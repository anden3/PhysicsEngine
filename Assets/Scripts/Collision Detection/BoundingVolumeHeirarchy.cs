/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 * 
 * Interesting link: https://www.codeproject.com/Articles/832957/Dynamic-Bounding-Volume-Hiearchy-in-Csharp
 */

using UnityEngine;

using System.Collections.Generic;

public struct PotentialContact
{
	public RigidBody[] body;

	public PotentialContact(RigidBody first, RigidBody second)
	{
		body = new RigidBody[2] { first, second };
	}
}

public class BVHNode
{
    public BVHNode parent = null;

    public BVHNode left;
    public BVHNode right;

    public RigidBody body;
    public BoundingVolume volume;

    public BVHNode(BVHNode parent, RigidBody body, BoundingVolume volume)
    {
        this.parent = parent;
        this.body = body;
        this.volume = volume;
    }

	public bool IsLeaf() => body != null;
	public bool Overlaps(BVHNode other) => volume.Overlaps(other.volume);

	public void GetPotentialContacts(ref List<PotentialContact> contacts)
	{
		if (IsLeaf()) return;

		left.GetPotentialContactsWith(right, ref contacts);
	}

	public void GetPotentialContactsWith(BVHNode other, ref List<PotentialContact> contacts)
	{
		if (!Overlaps(other)) return;

		if (IsLeaf() && other.IsLeaf())
		{
			contacts.Add(new PotentialContact(body, other.body));
			return;
		}

		if (other.IsLeaf()
			|| (!IsLeaf() && volume.GetSize() >= other.volume.GetSize()))
		{
			left.GetPotentialContactsWith(other, ref contacts);
			right.GetPotentialContactsWith(other, ref contacts);
		}
		else
		{
			GetPotentialContactsWith(other.left, ref contacts);
			GetPotentialContactsWith(other.right, ref contacts);
		}
	}

    public void RecalculateBoundingVolume()
    {
        // Generate a volume incorporating both children.
        throw new System.NotImplementedException();
    }

    public BVHNode GetSibling()
    {
        if (parent == null)      return null;
        if (parent.left == this) return parent.right;
        else                     return parent.left;
    }

    // Pick child that will grow the least.
    public BVHNode GetChildWithLowestGrowth(BoundingVolume newVol)
    {
        if (IsLeaf()) return null;

        return (
              left.volume.GetGrowth(newVol)
            < right.volume.GetGrowth(newVol)
            ) ? left : right;
    }
}

public class BVHTree
{
    private SortedSet<BVHNode> nodes = new SortedSet<BVHNode>();

    public void Insert(BVHNode node, RigidBody newBody, BoundingVolume newVolume)
    {
        if (node.IsLeaf())
        {
            node.left = new BVHNode(node, node.body, node.volume);
            node.right = new BVHNode(node, newBody, newVolume);

            nodes.Add(node.left);
            nodes.Add(node.right);

            node.body = null;

            node.RecalculateBoundingVolume();
        }
        else
        {
            Insert(node.GetChildWithLowestGrowth(newVolume), newBody, newVolume);
        }
    }

    public void Remove(BVHNode node)
    {
        if (node.parent != null)
        {
            BVHNode parent = node.parent;
            BVHNode sibling = node.GetSibling();

            // Copy sibling data to parent.
            parent.body = sibling.body;
            parent.volume = sibling.volume;
            parent.left = sibling.left;
            parent.right = sibling.right;

            nodes.Remove(sibling);

            parent.RecalculateBoundingVolume();
        }

        nodes.Remove(node.left);
        nodes.Remove(node.right);
    }
}
