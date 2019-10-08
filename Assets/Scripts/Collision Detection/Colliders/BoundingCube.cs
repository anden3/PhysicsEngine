/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[AddComponentMenu("Rigid Body Physics/Colliders/Cube")]
public class BoundingCube : BoundingVolume
{
    public Bounds bounds;

    public override Type type => Type.Cube;
    public override Vector3 center => bounds.center;

    public override bool IsPointInside(Vector3 point) => bounds.Contains(point);
    public override float GetSize() => bounds.size.x * bounds.size.y * bounds.size.z;

    public override bool Overlaps(BoundingVolume other)
    {
        switch (other.type)
        {
            case Type.Sphere:
                BoundingSphere s = other as BoundingSphere;
                return s.IsPointInside(bounds.ClosestPoint(s.center));

            case Type.Cube:
                BoundingCube c = other as BoundingCube;

                return (bounds.min.x <= c.bounds.max.x && bounds.max.x >= c.bounds.min.x) &&
                    (bounds.min.y <= c.bounds.max.y && bounds.max.y >= c.bounds.min.y) &&
                    (bounds.min.z <= c.bounds.max.z && bounds.max.z >= c.bounds.min.z);
            default:
                throw new System.NotImplementedException();
        }
    }
}
