/*
 * Based on https://www.gamedev.net/articles/programming/general-and-gameplay-programming/introduction-to-octrees-r3529/
 */

using UnityEngine;

using System.Linq;
using System.Collections.Generic;

using AndreExtensions;

public class Octree
{
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
    public static int objectCount = 0; // temp

    public static bool treeBuilt = false;
    public static bool treeReady = false;

    public static Vector3 minSize = Vector3.one;

    private static readonly List<Octree> nodes = new List<Octree>();
    private static readonly Queue<Primitive> pendingInsertion = new Queue<Primitive>();

    public Bounds region;
    public List<Primitive> objects;

    private readonly Octree parent;

    private readonly Octree[] children = new Octree[8];
    private readonly Bounds[] octants = new Bounds[8];
    private byte activeNodes = 0;

    private int maxLifeSpan = 8;
    private int currentLife = -1;

    private bool IsRoot() => parent == null;
    private bool IsEmpty() => objects.Count == 0;
    private bool IsLeaf() => activeNodes == 0;

    /// <summary>
    /// Creates an Octree with a suggestion for the bounding <paramref name="region"/> containing the items.
    /// </summary>
    /// <param name="region">The suggested dimensions for the bounding region.</param>
    /// <param name="parent">The parent of the node.</param>
    /// <remarks>If items are outside the <paramref name="region"/>, the <paramref name="region"/> will be automatically resized.</remarks>
    public Octree(Bounds region, Octree parent = null) : this(region, new List<Primitive>(), parent) { }

    /// <summary>
    /// Creates an Octree which encloses the given <paramref name="region"/> and contains the provided <paramref name="objects"/>.
    /// </summary>
    /// <param name="region">The bounding region for the oct tree.</param>
    /// <param name="objects">The list of objects contained within the bounding region.</param>
    /// <param name="parent">The parent of the node.</param>
    private Octree(Bounds region, List<Primitive> objects, Octree parent = null)
    {
        this.region = region;
        this.objects = objects;
        this.parent = parent;

        objectCount += objects.Count;

        currentLife = -1;

        if (parent == null && root != null)
            throw new System.InvalidOperationException("Attempted to create root node when a root node already exists.");

        if (root == null)
        {
            if (parent != null)
                throw new System.InvalidOperationException("Root node has parent!?");

            root = this;

            FindEnclosingCube(ref region);
        }


        nodes.Add(this);
        GetOctants(ref octants);
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

        UpdateLife();
        RemoveInactiveChildren();

        // Prune tree.
        objectCount -= objects.RemoveAll(o => o == null || !o.gameObject.activeInHierarchy);
        List<Primitive> movedObjects = objects.Where(p => p.HasBody() && p.body.Integrate(deltaTime)).ToList();

        foreach ((int index, Octree child) in GetActiveChildren())
            child.Update(deltaTime);

        // Move moved objects into the right node.
        foreach (Primitive movedObj in movedObjects)
        {
            if (FindBestFit(movedObj, out Octree node) && node != this)
            {
                objects.Remove(movedObj);
                node.objects.Add(movedObj);
            }
        }
    }

    public static void Enqueue_Static(Primitive item)
    {
        pendingInsertion.Enqueue(item);
        treeReady = false;
    }

    /// <summary>
    /// Add an <paramref name="item"/> to the tree at the next update.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Enqueue(Primitive item)
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
            if (FindBestFit(item, out Octree node))
            {
                node.objects.Add(item);
                objectCount++;
            }
            else
                throw new System.ArgumentOutOfRangeException("Item doesn't fit in the octree.");
        }
    }
    /// <summary>
    /// Add <paramref name="items"/> to the tree at the next update.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void Enqueue(IEnumerable<Primitive> items)
    {
        foreach (Primitive item in items)
            Enqueue(item);
    }

    /// <summary>
    /// Add <paramref name="items"/> to the tree at the next update.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void Enqueue(params Primitive[] items) => Enqueue(items);

    public void GetContacts(List<Contact> contacts) => GetContacts(new List<Primitive>(), contacts);

    /// <summary>
    /// Fill list with contacts of all objects that are colliding with each other in the tree.
    /// </summary>
    /// <param name="parentObjects">List of all objects higher up in the tree.</param>
    /// <param name="contacts">The list to be filled.</param>
    private void GetContacts(List<Primitive> parentObjects, List<Contact> contacts)
    {
        if (!IsEmpty())
        {
            foreach (Primitive parentObj in parentObjects)
                foreach (Primitive localObj in objects)
                    parentObj.GetContacts(localObj, contacts);
        }

        if (objects.Count > 1)
        {
            List<Primitive> temp = new List<Primitive>(objects);

            while (temp.Count > 0)
            {
                foreach (Primitive localObj in temp)
                {
                    Primitive other = temp[temp.Count - 1];

                    // Don't check against ourselves.
                    if (other != localObj)
                        other.GetContacts(localObj, contacts);
                }

                temp.RemoveAt(temp.Count - 1);
            }
        }

        parentObjects.AddRange(objects/*.Where(o => !o.isStationary)*/);

        foreach ((int index, Octree child) in GetActiveChildren())
            child.GetContacts(parentObjects, contacts);
    }

    private void BuildTree()
    {
        if (objects.Count == 0)
            return;

        if (region.size == Vector3.zero)
            throw new System.ArgumentException("Region size is zero", "region.size");

        if (region.size.LessThanOrEqual(minSize))
            // Can't fit any children.
            return;

        // Create lists of items to be added to the children.
        List<Primitive>[] octList = new List<Primitive>[8];

        for (int i = 0; i < 8; i++)
            octList[i] = new List<Primitive>();

        // Objects that get moved into children.
        List<Primitive> delist = new List<Primitive>();

        foreach (Primitive body in objects)
        {
            if (TryFitItemInChildren(body, out int index))
            {
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

        // treeBuilt = true;
        // treeReady = true;
    }

    /// <summary>
    /// Processes pending insertions.
    /// </summary>
    private void UpdateTree()
    {
        if (!treeBuilt)
        {
            while (pendingInsertion.Count > 0)
            {
                objects.Add(pendingInsertion.Dequeue());
                objectCount++;
            }

            BuildTree();

            treeBuilt = true;
            treeReady = true;
        }
        else
        {
            while (pendingInsertion.Count > 0)
            {
                Primitive item = pendingInsertion.Dequeue();

                if (FindBestFit(item, out Octree node))
                {
                    node.objects.Add(item);
                    objectCount++;
                }
            }  
        }

        treeReady = true;
    }

    /// <summary>
    /// Goes through the tree and stops at the first <paramref name="node"/> which fits the <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The item to fit.</param>
    /// <param name="node">The node that fits the item, is null if none are found.</param>
    /// <returns>True if node found, else false.</returns>
    private bool FindBestFit(Primitive item, out Octree node)
    {
        // Items without a rigidbody, such as planes, should be added to the root.
        if (!item.HasBody())
        {
            node = root;
            return true;
        }

        if (region.Contains(item.body.volume) != ContainmentType.Contains)
        {
            // Keep going upwards until item fits, or we hit the root node.
            if (IsRoot())
            {
                // Root region cannot contain object.
                // TODO: Need to rebuild entire tree...
                node = null;
                return false;
            }

            return parent.FindBestFit(item, out node);
        }

        if ((IsLeaf() && objects.Count < 2) || region.size.LessThanOrEqual(minSize))
        {
            node = this;
            return true;
        }

        if (TryFitItemInChildren(item, out int index))
        {
            if (TryGetChild(index, out Octree child))
                return child.FindBestFit(item, out node);

            return AddChild(index).FindBestFit(item, out node);
        }

        // Can't fit in any children.
        node = this;
        return true;
    }

    /// <summary>
    /// Decrease life of empty leaf nodes.
    /// </summary>
    private void UpdateLife()
    {
        if (IsEmpty())
        {
            if (IsLeaf())
            {
                if (currentLife == -1)
                    currentLife = maxLifeSpan;
                else
                    currentLife--;
            }
        }
        else if (currentLife > -1)
        {
            if (maxLifeSpan <= 64)
                maxLifeSpan *= 2;

            currentLife--;
        }
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
                activeNodes &= (byte)~(1 << index);
                nodes.Remove(child);
            }
        }
    }

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

    private Octree AddChild(int index) => AddChild(index, new List<Primitive>());
    private Octree AddChild(int index, List<Primitive> items)
    {
        activeNodes |= (byte)(1 << index);

        Octree child = new Octree(octants[index], items, this);
        children[index] = child;
        return child;
    }

    /// <summary>
    /// Try to fit <paramref name="item"/> into one of the octants given.
    /// </summary>
    /// <param name="item">The item to fit.</param>
    /// <param name="index">The index of the octant if one is found, else -1.</param>
    /// <returns>True if an octant that fits the item is found.</returns>
    private bool TryFitItemInChildren(Primitive item, out int index)
    {
        if (item.body == null)
        {
            index = -1;
            return false;
        }

        for (index = 0; index < octants.Length; index++)
            if (octants[index].Contains(item.body.volume) == ContainmentType.Contains)
                return true;

        index = -1;
        return false;
    }

    /// <summary>
    /// Get all children which are active.
    /// </summary>
    /// <returns>Enumerator to index and instance of child.</returns>
    private IEnumerable<(byte index, Octree child)> GetActiveChildren()
    {
        if (activeNodes == 0)
            yield break;

        for (byte i = 0; i < 8; i++)
            if (TryGetChild(i, out Octree child))
                yield return (i, child);
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

    /*
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
    */

    public static void Debug_Draw()
    {
        foreach (Octree node in nodes)
        {
            Gizmos.color = node.IsEmpty() ? Color.green : Color.red;
            Gizmos.DrawWireCube(node.region.center, node.region.size);
        }
    }
}
