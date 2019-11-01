/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using AndreExtensions;

[AddComponentMenu("Rigid Body Physics/Colliders/Sphere")]
public class BoundingSphere : BoundingVolume
{
    public bool fitMesh = true;
	public float radius {
        get => _radius * transform.localScale.GetMax();
        set => _radius = value;
    }

    [SerializeField]
    private float _radius;

    public override Type type => Type.Sphere;
    public override Vector3 center => transform.position;
    public override float size => _size;
    private float _size;

    private void OnValidate()
    {
        if (fitMesh && TryGetComponent(out MeshRenderer mesh))
        {
            radius = mesh.bounds.extents.GetMax();
        }
    }

    protected void Awake()
    {
        // base.Awake();
        _size = 4.0f / 3 * Mathf.PI * Mathf.Pow(radius, 3);
    }

    public override bool IsPointInside(Vector3 point)
        => (point - center).sqrMagnitude <= (radius * radius);

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

    public override bool Overlaps(BoundingSphere s, out Contact contact)
    {
        float minDist = radius + s.radius;

        Vector3 midLine = s.center - center;
        float distSqr = midLine.sqrMagnitude;

        if (distSqr > minDist.Squared())
        {
            contact = null;
            return false;
        }

        float dist = Mathf.Sqrt(distSqr);

        float depth = minDist - dist;
        Vector3 normal = midLine / dist;
        Vector3 position = center + midLine * 0.5f;

        contact = new Contact(
            body, s.body, position, normal, depth
        );

        return true;
    }

    public override bool Overlaps(BoundingCube c, out Contact contact)
    {
        Vector3 closestPoint = c.bounds.ClosestPoint(center);

        if (!IsPointInside(closestPoint))
        {
            contact = null;
            return false;
        }

        Vector3 toCenter = center - closestPoint;
        float distToCenter = toCenter.magnitude;
        float depth = radius - distToCenter;

        Vector3 normal = toCenter / distToCenter;
        Vector3 position = closestPoint;

        contact = new Contact(
            body, c.body, position, normal, depth
        );

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        // Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(center, radius);
    }
}
