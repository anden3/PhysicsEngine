using UnityEngine;

public class BoundingSphere : BoundingVolume
{
	public Vector3 center;
	public float radius;

	public override float GetSize()
	{
		return radius * radius * Mathf.PI;
	}

	public override bool Overlaps(BoundingVolume other)
	{
		return base.Overlaps(other);
	}
}
