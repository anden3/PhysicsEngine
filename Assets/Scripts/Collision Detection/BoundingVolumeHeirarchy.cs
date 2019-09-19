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

    public void Insert(RigidBody newBody, BoundingVolume newVolume)
    {
        if (IsLeaf())
        {
            left = new BVHNode(this, body, volume);
            right = new BVHNode(this, newBody, newVolume);

            body = null;

            RecalculateBoundingVolume();
        }
        else
        {
            // Pick child that will grow the least.
            if ( left.volume.GetGrowth(newVolume) <
                right.volume.GetGrowth(newVolume))
            {
                left.Insert(newBody, newVolume);
            }
            else
            {
                right.Insert(newBody, newVolume);
            }
        }
    }

    public void Remove()
    {
        if (parent != null)
        {
            BVHNode sibling = GetSibling();

            // Copy sibling data to parent.
            parent.body = sibling.body;
            parent.volume = sibling.volume;
            parent.left = sibling.left;
            parent.right = sibling.right;

            // "Delete" sibling. This is C# tho, so can't do that.
            // Let's just hope the GC gets it.

            parent.RecalculateBoundingVolume();
        }

        // "Delete" children.
    }

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

    private void RecalculateBoundingVolume()
    {
        // Generate a volume incorporating both children.
        throw new System.NotImplementedException();
    }

    private BVHNode GetSibling()
    {
        if (parent == null)      return null;
        if (parent.left == this) return parent.right;
        else                     return parent.left;
    }
}
