/*
 * Based on https://www.gamedev.net/articles/programming/general-and-gameplay-programming/introduction-to-octrees-r3529/
 */

using UnityEngine;

using System.Linq;
using System.Collections.Generic;

using AndreExtensions;

public class Octree
{
    private static readonly Vector3 MIN_SIZE = Vector3.one;

    private static readonly Vector3[] OCTANT_CENTERS = new Vector3[8]
    {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, -.5f, +.5f),
        new Vector3(-.5f, +.5f, -.5f),
        new Vector3(-.5f, +.5f, +.5f),
        new Vector3(+.5f, -.5f, -.5f),
        new Vector3(+.5f, -.5f, +.5f),
        new Vector3(+.5f, +.5f, -.5f),
        new Vector3(+.5f, +.5f, +.5f)
    };

    public static Octree root = null;

    public static bool treeBuilt = false;
    public static bool treeReady = false;

    private static readonly List<Octree> nodes = new List<Octree>();
    private static readonly Queue<RigidBody> pendingInsertion = new Queue<RigidBody>();

    public Bounds region;
    public List<RigidBody> objects;

    private Octree parent;

    private readonly Octree[] children = new Octree[8];
    private readonly Bounds[] octants = new Bounds[8];
    private byte activeNodes = 0;

    private int maxLifeSpan = 8;
    private int currentLife = -1;

    private bool hasChildren = false;

    private bool IsRoot() => parent == null;

    private bool IsEmpty() => objects.Count == 0;

    private bool IsLeaf() => activeNodes == 0;

    /*
    /// <summary>
    /// Creates an Octree.
    /// </summary>
    /// <param name="parent">The parent of the node.</param>
    public Octree(Octree parent = null) : this(new Bounds(Vector3.zero, Vector3.zero), new List<RigidBody>(), parent) { }
    */

    /// <summary>
    /// Creates an Octree with a suggestion for the bounding <paramref name="region"/> containing the items.
    /// </summary>
    /// <param name="region">The suggested dimensions for the bounding region.</param>
    /// <param name="parent">The parent of the node.</param>
    /// <remarks>If items are outside the <paramref name="region"/>, the <paramref name="region"/> will be automatically resized.</remarks>
    public Octree(Bounds region, Octree parent = null) : this(region, new List<RigidBody>(), parent) { }

    /// <summary>
    /// Creates an Octree which encloses the given <paramref name="region"/> and contains the provided <paramref name="objects"/>.
    /// </summary>
    /// <param name="region">The bounding region for the oct tree.</param>
    /// <param name="objects">The list of objects contained within the bounding region.</param>
    /// <param name="parent">The parent of the node.</param>
    private Octree(Bounds region, List<RigidBody> objects, Octree parent = null)
    {
        this.region = region;
        this.objects = objects;
        currentLife = -1;

        if (parent == null && root != null)
            throw new System.InvalidOperationException("Attempted to create root node when a root node already exists.");

        if (root == null)
        {
            if (parent != null)
                throw new System.InvalidOperationException("Root node has parent!?");

            root = this;
            nodes.Add(this);
            FindEnclosingCube(ref region);
            GetOctants(ref octants);
        }  
    }

    public void Update(float deltaTime)
    {
        if (!treeBuilt || !treeReady)
        {
            // Build tree if necessary and insert new objects before updating.
            if (pendingInsertion.Count > 0)
            {
                UpdateTree();

                // Ah shit, here we go again...
                Update(deltaTime);
            }

            return;
        }

        // Start countdown for leaf nodes.
        if (objects.Count == 0)
        {
            if (!hasChildren)
            {
                if (currentLife == -1)
                    currentLife = maxLifeSpan;
                else if (currentLife > 0)
                    currentLife--;
            }
        }
        else
        {
            if (currentLife != -1)
            {
                if (maxLifeSpan <= 64)
                    maxLifeSpan *= 2;

                currentLife--;
            }
        }

        // Prune tree.
        objects.RemoveAll(o => o == null || !o.gameObject.activeInHierarchy);
        List<RigidBody> movedObjects = objects.Where(rb => rb.Integrate(deltaTime)).ToList();

        RemoveInactiveChildren();

        foreach ((int index, Octree child) in GetActiveChildren())
            child.Update(deltaTime);

        // Move moved objects into the right node.
        foreach (RigidBody movedObj in movedObjects)
        {
            if (FindBestFit(movedObj, this, out Octree node))
            {
                objects.Remove(movedObj);
                node.objects.Add(movedObj);
            }
        }
    }

    public static void Enqueue_Static(RigidBody item)
    {
        pendingInsertion.Enqueue(item);
        treeReady = false;
    }

    /// <summary>
    /// Add an <paramref name="item"/> to the tree at the next update.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Enqueue(RigidBody item)
    {
        if (IsRoot())
        {
            // Insert item at next update.
            pendingInsertion.Enqueue(item);
            treeReady = false;
        }
        else
        {
            // Since the item isn't being added to the root,
            // we will take that as a hint that it should be placed near the current node.
            if (FindBestFit(item, this, out Octree node))
                node.objects.Add(item);
            else
                throw new System.ArgumentOutOfRangeException("Item doesn't fit in the octree.");
        }
    }
    /// <summary>
    /// Add <paramref name="items"/> to the tree at the next update.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void Enqueue(IEnumerable<RigidBody> items)
    {
        foreach (RigidBody item in items)
            Enqueue(item);
    }

    /// <summary>
    /// Add <paramref name="items"/> to the tree at the next update.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void Enqueue(params RigidBody[] items) => Enqueue(items);

    /// <summary>
    /// Processes pending insertions.
    /// </summary>
    private void UpdateTree()
    {
        if (!treeBuilt)
        {
            while (pendingInsertion.Count > 0)
                objects.Add(pendingInsertion.Dequeue());

            BuildTree();
        }
        else
        {
            while (pendingInsertion.Count > 0)
                Insert(pendingInsertion.Dequeue());
        }

        treeReady = true;
    }

    private void BuildTree()
    {
        if (objects.Count == 0)
            return;

        if (region.size == Vector3.zero)
            // FindEnclosingCube(ref region);
            throw new System.ArgumentException("Region size is zero", "region.size");
        
        if (region.size.LessThanOrEqual(MIN_SIZE))
            // Can't fit any children.
            return;

        // Create lists of items to be added to the children.
        List<RigidBody>[] octList = new List<RigidBody>[8];

        for (int i = 0; i < 8; i++)
            octList[i] = new List<RigidBody>();

        // Objects that get moved into children.
        List<RigidBody> delist = new List<RigidBody>();

        foreach (RigidBody body in objects)
        {
            if (TryFitItemInChildren(body, out int index))
            {
                hasChildren = true;
                octList[index].Add(body);
                delist.Add(body);
            }
        }

        objects.RemoveAll(delist.Contains);

        for (int i = 0; i < 8; i++)
        {
            if (octList[i].Count > 0)
            {
                children[i] = AddChild(i, octList[i]);
                children[i].BuildTree();
            }
        }

        treeBuilt = true;
        treeReady = true;
    }

    private void Insert(RigidBody item)
    {
        if (FindBestFit(item, this, out Octree node))
            node.objects.Add(item);
    }

    public List<Contact> GetContacts() => GetContacts(new List<RigidBody>());

    // Get a list with contacts of all objects that are colliding with each other in the tree.
    private List<Contact> GetContacts(List<RigidBody> parentObjects)
    {
        List<Contact> contacts = new List<Contact>();

        if (!IsEmpty())
        {
            foreach (RigidBody parentObj in parentObjects)
            {
                foreach (RigidBody localObj in objects)
                {
                    if (parentObj.volume.Overlaps(localObj.volume, out Contact contact))
                    {
                        contacts.Add(contact);
                    }
                }
            }
        }

        if (objects.Count > 1)
        {
            List<RigidBody> temp = new List<RigidBody>(objects);

            while (temp.Count > 0)
            {
                foreach (RigidBody localObj in temp)
                {
                    RigidBody other = temp[temp.Count - 1];

                    // Don't check stationary objects or against ourselves.
                    if (other == localObj || (other.isStationary && localObj.isStationary))
                        continue;

                    if (other.volume.Overlaps(localObj.volume, out Contact contact))
                    {
                        contacts.Add(contact);
                    }
                }

                temp.RemoveAt(temp.Count - 1);
            }
        }

        parentObjects.AddRange(objects.Where(o => !o.isStationary));

        foreach ((int index, Octree child) in GetActiveChildren())
            contacts.AddRange(child.GetContacts(parentObjects));

        return contacts;
    }

    private static void UnloadTree()
    {
        pendingInsertion.Clear();

        foreach (Octree node in nodes)
        {
            if (node == null)
                continue;

            node.objects.Clear();
            node.region.center = Vector3.zero;
            node.region.size = Vector3.zero;

            for (int i = 0; i < 8; i++)
                node.children[i] = null;

            node.activeNodes = 0;
            node.parent = null;
        }

        nodes.Clear();

        treeBuilt = false;
        treeReady = false;
    }

    /// <summary>
    /// Prunes empty children with no life remaining.
    /// </summary>
    private void RemoveInactiveChildren()
    {
        foreach ((byte index, Octree child) in GetActiveChildren()
            .Where(c => c.child.currentLife == 0))
        {
            if (!child.IsEmpty())
            {
                child.currentLife = -1;
            }
            else
            {
                children[index] = null;
                activeNodes ^= (byte)(1 << index);
                // TODO: Check if the removal should happen recursively.
                // nodes.Remove(child);
            }
        }
    }

    /// <summary>
    /// Get bounds resulting from splitting the current region into <paramref name="octants"/>.
    /// </summary>
    /// <param name="octants">The array to fill with the bounds of the octants.</param>
    private void GetOctants(ref Bounds[] octants)
    {
        for (int i = 0; i < 8; i++)
            octants[i] = GetNewOctant(i, region);
    }

    private Bounds GetNewOctant(int index, Bounds parent)
        => new Bounds(parent.center + Vector3.Scale(OCTANT_CENTERS[index], parent.extents), parent.extents);

    private bool TryGetChild(int index, out Octree child)
    {
        if ((activeNodes & (1 << index)) > 0)
        {
            child = children[index];
            Debug.Assert(child != null);
            return true;
        }

        child = null;
        return false;
    }

    private Octree AddChild(int index) => AddChild(index, new List<RigidBody>());
    private Octree AddChild(int index, List<RigidBody> items)
    {
        activeNodes |= (byte)(1 << index);
        nodes.Add(new Octree(octants[index], items, this));
        return nodes.Back();
    }

    /// <summary>
    /// Goes through the tree and stops at the first <paramref name="node"/> which fits the <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The item to fit.</param>
    /// <param name="start">The node to start from.</param>
    /// <param name="node">The node that fits the item, is null if none are found.</param>
    /// <returns>True if node found, else false.</returns>
    private static bool FindBestFit(RigidBody item, Octree start, out Octree node)
    {
        node = start;

        while (true)
        {
            if (node.region.Contains(item.volume) != ContainmentType.Contains)
            {
                // Keep going upwards until item fits, or we hit the root node.
                if (node.IsRoot())
                {
                    // Root region cannot contain object.
                    // Need to rebuild entire tree...
                    node = null;
                    return false;
                }

                node = node.parent;
                continue;
            }

            if ((node.IsLeaf() && node.objects.Count < 2) || node.region.size.LessThanOrEqual(MIN_SIZE))
            {
                return true;
            }

            if (node.TryFitItemInChildren(item, out int index))
            {
                if (node.TryGetChild(index, out Octree child))
                {
                    node = child;
                    continue;
                }

                node = node.AddChild(index);
                return true;
            }

            // Can't fit in any children.
            return true;
        }
    }

    /// <summary>
    /// Try to fit item into one of the octants given.
    /// </summary>
    /// <param name="item">The item to fit.</param>
    /// <param name="octants">An array of bounds representing the octants.</param>
    /// <param name="index">The index of the octant if one is found, else -1.</param>
    /// <returns>True if an octant that fits the item is found.</returns>
    private bool TryFitItemInChildren(RigidBody item, out int index)
    {
        for (index = 0; index < octants.Length; index++)
            if (octants[index].Contains(item.volume) == ContainmentType.Contains)
                return true;

        index = -1;
        return false;
    }

    /// <summary>
    /// Get all children which are active and not null.
    /// </summary>
    /// <returns>Enumerator to index and instance of child.</returns>
    private IEnumerable<(byte index, Octree child)> GetActiveChildren()
    {
        if (activeNodes == 0)
            yield break;

        byte b = activeNodes;

        for (byte i = 0; i < 8; b >>= 1, i++)
            if ((b & 1) == 1 && children[i] != null)
                yield return (i, children[i]);
    }

    public static void Debug_Draw()
    {
        foreach (Octree node in nodes)
        {
            Gizmos.color = node.IsEmpty() ? Color.green : Color.red;
            Gizmos.DrawWireCube(node.region.center, node.region.size);
        }
    }

    /// <summary>
    /// This finds the smallest enclosing cube with power of 2 dimensions for the given bounding box
    /// </summary>
    /// <param name="region">The bounding box to cubify</param>
    /// <returns>A cubified version of the input parameter.</returns>
    public static void FindEnclosingCube(ref Bounds region)
    {
        int newSize = Mathf.CeilToInt(region.size.GetMax());

        if (!Mathf.IsPowerOfTwo(newSize))
            newSize = Mathf.NextPowerOfTwo(newSize);

        region.size = new Vector3(newSize, newSize, newSize);
    }
}
