/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[AddComponentMenu("Rigid Body Physics/Colliders/Sphere")]
public class BoundingSphere : BoundingVolume
{
	public Vector3 center;
	public float radius;

    public override Type type => Type.Sphere;

    public override float GetSize() => radius * radius * Mathf.PI;

    public override bool Overlaps(BoundingVolume other)
	{
        switch (other.type)
        {
            case Type.Sphere:
                BoundingSphere otherSphere = other as BoundingSphere;

                float distSqr = (center - otherSphere.center).sqrMagnitude;
                float minDistSqr = (radius + otherSphere.radius) * (radius + otherSphere.radius);
                return distSqr < minDistSqr;

            default:
                throw new System.NotImplementedException();
        }
	}
}
