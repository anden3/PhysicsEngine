/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[AddComponentMenu("Rigid Body Physics/Colliders/Sphere")]
public class BoundingSphere : BoundingVolume
{
	public float radius;

    public override Type type => Type.Sphere;
    public override Vector3 center { get; }
    public override float GetSize() => radius * radius * Mathf.PI;

    public override bool IsPointInside(Vector3 point)
    {
        float distance = 
            (point.x - center.x) * (point.x - center.x) +
            (point.y - center.y) * (point.y - center.y) +
            (point.z - center.z) * (point.z - center.z);

        return distance < (radius * radius);
    }

    public override bool Overlaps(BoundingVolume other)
	{
        switch (other.type)
        {
            case Type.Sphere:
                BoundingSphere s = other as BoundingSphere;

                float distSqr = (center - s.center).sqrMagnitude;
                float minDistSqr = (radius + s.radius) * (radius + s.radius);
                return distSqr < minDistSqr;

            case Type.Cube:
                BoundingCube c = other as BoundingCube;
                return IsPointInside(c.bounds.ClosestPoint(center));

            default:
                throw new System.NotImplementedException();
        }
	}
}
