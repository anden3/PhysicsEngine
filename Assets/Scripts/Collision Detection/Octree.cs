/*
 * Based on https://www.gamedev.net/articles/programming/general-and-gameplay-programming/introduction-to-octrees-r3529/
 */

using UnityEngine;

using System.Collections.Generic;

public class Octree
{
    private const int MIN_SIZE = 1;

    public static bool treeBuilt = false;
    public static bool treeReady = false;

    private static Queue<RigidBody> pendingInsertion = new Queue<RigidBody>();

    public Bounds region;
    public List<RigidBody> objects;

    private Octree parent;

    private Octree[] children = new Octree[8];
    private byte activeNodes = 0;

    private int maxLifeSpan = 8;
    private int currentLife = -1;

    private bool hasChildren = false;

    private bool IsRoot() => parent == null;
    private bool IsLeaf() => objects.Count <= 1;

    /// <summary>
    /// Creates an Octree.
    /// </summary>
    public Octree()
    {
        region = new Bounds(Vector3.zero, Vector3.zero);
        objects = new List<RigidBody>();
        currentLife = -1;
    }

    /// <summary>
    /// Creates an Octree with a suggestion for the bounding region containing the items.
    /// </summary>
    /// <param name="region">The suggested dimensions for the bounding region.</param>
    /// <remarks>If items are outside this region, the region will be automatically resized.</remarks>
    public Octree(Bounds region)
    {
        this.region = region;
        objects = new List<RigidBody>();
        currentLife = -1;
    }

    /// <summary>
    /// Creates an oct tree which encloses the given region and contains the provided objects.
    /// </summary>
    /// <param name="region">The bounding region for the oct tree.</param>
    /// <param name="objects">The list of objects contained within the bounding region.</param>
    private Octree(Bounds region, List<RigidBody> objects)
    {
        this.region = region;
        this.objects = objects;
        currentLife = -1;
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
            }

            BuildTree();
        }
        else
        {
            while (pendingInsertion.Count > 0)
            {
                Insert(pendingInsertion.Dequeue());
            }
        }

        treeReady = true;
    }

    private void BuildTree()
    {
        if (IsLeaf())
            return;

        if (region.size == Vector3.zero)
            FindEnclosingCube();

        if (region.size.x <= MIN_SIZE && region.size.y <= MIN_SIZE && region.size.z <= MIN_SIZE)
            return;

        Vector3 min = region.min;
        Vector3 max = region.max;
        Vector3 center = region.center;

        Bounds[] octant = new Bounds[8];

        octant[0] = new Bounds(min, center);
        octant[1] = new Bounds(new Vector3(center.x, min.y, min.z), new Vector3(max.x, center.y, center.z));
        octant[2] = new Bounds(new Vector3(center.x, min.y, center.z), new Vector3(max.x, center.y, max.z));
        octant[3] = new Bounds(new Vector3(min.x, min.y, center.z), new Vector3(center.x, center.y, max.z));
        octant[4] = new Bounds(new Vector3(min.x, center.y, min.z), new Vector3(center.x, max.y, center.z));
        octant[5] = new Bounds(new Vector3(center.x, center.y, min.z), new Vector3(max.x, max.y, center.z));
        octant[6] = new Bounds(center, max);
        octant[7] = new Bounds(new Vector3(min.x, center.y, center.z), new Vector3(center.x, max.y, max.z));

        List<RigidBody>[] octList = new List<RigidBody>[8];

        for (int i = 0; i < 8; i++)
            octList[i] = new List<RigidBody>();

        // Objects that get moved into octants.
        List<RigidBody> delist = new List<RigidBody>();

        foreach (RigidBody body in objects)
        {
            for (int i = 0; i < 8; i++)
            {
                if (octant[i].Contains(body.volume.center))
                {
                    hasChildren = true;
                    octList[i].Add(body);
                    delist.Add(body);
                    break;
                }
            }
        }

        foreach (RigidBody body in delist)
        {
            objects.Remove(body);
        }

        for (int i = 0; i < 8; i++)
        {
            if (octList[i].Count > 0)
            {
                children[i] = CreateNode(octant[i], octList[i]);
                activeNodes |= (byte)(1 << i);
                children[i].BuildTree();
            }
        }

        treeBuilt = true;
        treeReady = true;
    }

    private Octree CreateNode(Bounds region, List<RigidBody> objects)
    {
        if (objects.Count == 0)
            return null;

        return new Octree(region, objects)
        {
            parent = this
        };
    }

    private Octree CreateNode(Bounds region, RigidBody body)
    {
        return new Octree(region, new List<RigidBody> { body })
        {
            parent = this
        };
    }

    public void Update(float deltaTime)
    {
        if (!treeBuilt || !treeReady)
        {
            if (pendingInsertion.Count > 0)
            {
                UpdateTree();
                Update(deltaTime);
                return;
            }
        }

        // Start countdown for leaf nodes.
        if (objects.Count == 0)
        {
            if (!hasChildren)
            {
                if (currentLife == -1)
                {
                    currentLife = maxLifeSpan;
                }
                else if (currentLife > 0)
                {
                    currentLife--;
                }
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

        // Find objects that have moved.
        List<RigidBody> movedObjects = new List<RigidBody>(objects.Count);

        foreach (RigidBody body in objects)
        {
            if (body.Integrate(deltaTime))
            {
                movedObjects.Add(body);
            }
        }

        // Remove inactive objects.
        int count = movedObjects.Count;

        for (int i = 0; i < count; i++)
        {
            if (!objects[i].gameObject.activeInHierarchy)
            {
                if (movedObjects.Contains(objects[i]))
                {
                    movedObjects.Remove(objects[i]);
                }

                objects.RemoveAt(i--);
                count--;
            }
        }

        // Stop dead branches from being displayed as active.
        for (int flags = activeNodes, index = 0; flags > 0; flags >>= 1, index++)
        {
            if ((flags & 1) == 1 && children[index].currentLife == 0)
            {
                if (children[index].objects.Count > 0)
                {
                    children[index].currentLife = -1;
                }
                else
                {
                    children[index] = null;
                    activeNodes ^= (byte)(1 << index);
                }
            }
        }

        // Update child nodes.
        for (int flags = activeNodes, index = 0; flags > 0; flags >>= 1, index++)
        {
            if ((flags & 1) == 1)
            {
                if (children != null && children[index] != null)
                {
                    children[index].Update(deltaTime);
                }
            }
        }

        // Move moved objects into the right node.
        foreach (RigidBody movedObj in movedObjects)
        {
            Octree current = this;

            Vector3 volumeCenter = movedObj.volume.center;

            while (current.region.Contains(volumeCenter))
            {
                if (current.parent != null)
                    current = current.parent;
                else
                    break;
            }

            objects.Remove(movedObj);
            current.Insert(movedObj);
        }

        if (IsRoot())
        {

        }
    }
}
