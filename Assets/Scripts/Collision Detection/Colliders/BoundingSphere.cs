/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

using AndreExtensions;
using NaughtyAttributes;

[AddComponentMenu("Rigid Body Physics/Colliders/Sphere")]
public class BoundingSphere : BoundingVolume
{
    [OnValueChanged("FitMesh")]
    [ValidateInput("ValidateMesh", "Mesh doesn't exist!")]
    public bool fitMesh = true;

    [DisableIf("fitMesh")]
    [OnValueChanged("RadiusChanged")]
    public float radius;

    public override Type type => Type.Sphere;
    public override Vector3 center => transform.position;
    public override float size => _size;
    private float _size;

    private void Awake()
    {
        if (fitMesh)
            radius = GetComponent<MeshRenderer>().bounds.extents.GetMax();

        _size = 4.0f / 3 * Mathf.PI * Mathf.Pow(radius, 3);
    }

    private void RadiusChanged()
    {
        radius *= transform.localScale.GetMax();
        _size = 4.0f / 3 * Mathf.PI * Mathf.Pow(radius, 3);
    }

    private void FitMesh()
    {
        if (fitMesh && TryGetComponent(out MeshRenderer mesh))
            radius = mesh.bounds.extents.GetMax();
    }

    private bool ValidateMesh(bool useMesh)
    {
        if (useMesh && !TryGetComponent(out MeshRenderer mesh))
            return false;

        return true;
    }

    public override bool IsPointInside(Vector3 point)
        => (point - center).sqrMagnitude <= (radius * radius);

    public override bool GetContacts(BoundingVolume other, List<Contact> contacts)
	{
        switch (other.type)
        {
            case Type.Sphere:
                return GetContacts(other as BoundingSphere, contacts);

            case Type.Cube:
                return GetContacts(other as BoundingCube, contacts);

            default:
                throw new System.NotImplementedException();
        }
	}

    public override bool GetContacts(BoundingSphere s, List<Contact> contacts)
    {
        float minDist = radius + s.radius;

        Vector3 midLine = s.center - center;
        float distSqr = midLine.sqrMagnitude;

        if (distSqr > minDist.Squared())
            return false;

        float dist = Mathf.Sqrt(distSqr);

        float depth = minDist - dist;
        Vector3 normal = midLine / dist;
        Vector3 position = center + midLine * 0.5f;

        contacts.Add(new Contact(
            body, s.body, position, -normal, depth
        ));

        return true;
    }

    public override bool GetContacts(BoundingCube c, List<Contact> contacts)
    {
        Vector3 closestPoint = c.bounds.ClosestPoint(center);

        if (!IsPointInside(closestPoint))
            return false;

        Vector3 toCenter = center - closestPoint;
        float distToCenter = toCenter.magnitude;
        float depth = radius - distToCenter;

        Vector3 normal = toCenter / distToCenter;
        Vector3 position = closestPoint;

        contacts.Add(new Contact(
            body, c.body, position, normal, depth
        ));

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center, radius);
    }
}
