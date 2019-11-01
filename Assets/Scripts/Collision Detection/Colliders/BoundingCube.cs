/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using AndreExtensions;

[ExecuteInEditMode]
[AddComponentMenu("Rigid Body Physics/Colliders/Cube")]
public class BoundingCube : BoundingVolume
{
    public bool fitMesh = true;
    public Bounds bounds;

    public override Type type => Type.Cube;
    public override Vector3 center => bounds.center;
    public override float size => _size;
    private float _size;

    public override bool IsPointInside(Vector3 point) => bounds.Contains(point);
    public bool IsBoundsInside(Bounds b) =>
        bounds.min.LessThanOrEqual(b.max) && bounds.max.GreaterThanOrEqual(b.min);

    protected void Awake()
    {
        // base.Awake();
        _size = bounds.size.x * bounds.size.y * bounds.size.z;
    }

    private void Update()
    {
        if (transform.hasChanged && fitMesh && TryGetComponent(out MeshRenderer mesh))
        {
            bounds.extents = mesh.bounds.extents;
        }

        bounds.center = transform.position;
    }

    public override bool Overlaps(BoundingVolume other, out Contact contact)
    {
        switch (other.type)
        {
            case Type.Sphere:
                return Overlaps(other as BoundingSphere, out contact);

            case Type.Cube:
                return Overlaps(other as BoundingCube, out contact);

            default:
                throw new System.NotImplementedException();
        }
    }

    public override bool Overlaps(BoundingCube c, out Contact contact)
    {
        /*
        if (!IsBoundsInside(c.bounds))
        {
            contact = null;
            return false;
        }
        */

        // Source: https://blog.hamaluik.ca/posts/simple-aabb-collision-using-minkowski-difference/
        Bounds minkowskiDifference = bounds.GetMinikowskiDifference(c.bounds);

        if (!minkowskiDifference.Contains(Vector3.zero))
        {
            contact = null;
            return false;
        }

        Vector3 penetrationVector = minkowskiDifference.ClosestPoint(Vector3.zero);

        // Just guessing.
        float depth = penetrationVector.magnitude;
        Vector3 normal = penetrationVector / depth;
        Vector3 position = c.center + penetrationVector;
        // Vector3 normal = GetNormal(position);

        contact = new Contact(
            body, c.body, position, normal, depth
        );

        return true;
    }

    public override bool Overlaps(BoundingSphere s, out Contact contact)
    {
        Vector3 relCenter = transform.InverseTransformPoint(s.center);

        // Return false if distance to sphere's center is larger than the largest extent of the cube.
        if (relCenter.sqrMagnitude > bounds.extents.GetMax().Squared())
        {
            contact = null;
            return false;
        }

        // Separating axis theorem.
        if (Mathf.Abs(relCenter.x) - s.radius > bounds.extents.x ||
            Mathf.Abs(relCenter.y) - s.radius > bounds.extents.y ||
            Mathf.Abs(relCenter.z) - s.radius > bounds.extents.z)
        {
            contact = null;
            return false;
        }

        Vector3 closestPoint = bounds.ClosestPoint(s.center);

        if (!s.IsPointInside(closestPoint))
        {
            contact = null;
            return false;
        }

        Vector3 normal = GetNormal(closestPoint);
        Vector3 position = closestPoint;

        float depth = s.radius - (closestPoint - s.center).magnitude;

        contact = new Contact(
            body, s.body, position, normal, depth
        );

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private Vector3 GetNormal(Vector3 point)
    {
        Vector3 normal = Vector3.zero;

        float minDist = float.MaxValue;
        point -= center;

        Vector3 distance = (bounds.extents - point.Abs()).Abs();

        for (int i = 0; i < 3; ++i)
        {
            float dist = distance[i];

            if (dist < minDist)
            {
                minDist = dist;

                switch (i)
                {
                    case 0: normal = -Mathf.Sign(point.x) * Vector3.right; break;
                    case 1: normal = -Mathf.Sign(point.y) * Vector3.up; break;
                    case 2: normal = -Mathf.Sign(point.z) * Vector3.forward; break;
                }
            }
        }

        return normal;
    }
}
