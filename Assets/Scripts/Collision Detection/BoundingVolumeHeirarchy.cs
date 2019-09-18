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
	public BVHNode[] children = new BVHNode[2];
	public BoundingVolume volume;

	public RigidBody body;

	public bool IsLeaf() => body != null;
	public bool Overlaps(BVHNode other) => volume.Overlaps(other.volume);

	public void GetPotentialContacts(ref List<PotentialContact> contacts)
	{
		if (IsLeaf()) return;

		children[0].GetPotentialContactsWith(children[1], ref contacts);
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
			children[0].GetPotentialContactsWith(other, ref contacts);
			children[1].GetPotentialContactsWith(other, ref contacts);
		}
		else
		{
			GetPotentialContactsWith(other.children[0], ref contacts);
			GetPotentialContactsWith(other.children[1], ref contacts);
		}
	}
}
