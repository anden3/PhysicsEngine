/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 */

using UnityEngine;

using System.Collections.Generic;

using NaughtyAttributes;

[ExecuteInEditMode]
[AddComponentMenu("Rigid Body Physics/Plane")]
public class Plane : Primitive
{
    public MeshRenderer mesh;
    public Vector2 size;

    [ReadOnly] public Vector3 normal;
    [ReadOnly] public float offset;

    private void Awake()
    {
        isStationary = true;
    }

    private void Update()
    {
        normal = -transform.forward;
        offset = Vector3.Dot(normal, transform.position);

        mesh.transform.localScale = new Vector3(size.x, size.y, 1);
    }

    public bool IsPointInside(Vector3 point) => Vector3.Dot(normal, point) < offset;

    public override bool GetContacts(Primitive other, List<Contact> contacts)
    {
        if (!base.GetContacts(other, contacts))
            return false;

        switch (other.body.volume.type)
        {
            case BoundingVolume.Type.Sphere:
                BoundingSphere sphere = (BoundingSphere)other.body.volume;

                float distance = Vector3.Dot(normal, sphere.transform.position) - sphere.radius - offset;

                if (distance >= 0)
                    return false;

                Vector3 position = sphere.transform.position - normal * (distance + sphere.radius);

                contacts.Add(new Contact(other.body, null, position, normal, -distance));
                return true;

            case BoundingVolume.Type.Cube:
                BoundingCube cube = (BoundingCube)other.body.volume;

                // TODO: Make this work.
                /*
                // Source: https://gdbooks.gitbooks.io/3dcollisions/content/Chapter2/static_aabb_plane.html
                float projectedRadius = Vector3.Dot(cube.bounds.extents, normal.Abs());
                float dist = Vector3.Dot(normal, cube.bounds.center) - offset;

                if (Mathf.Abs(dist) > projectedRadius)
                {
                    return false;
                }

                contacts.add(new Contact(other.body, null, ))
                return true;
                */

                bool overlap = false;

                foreach (Vector3 vertex in cube.GetVertices())
                {
                    if (IsPointInside(vertex))
                    {
                        overlap = true;

                        float dist = Vector3.Dot(normal, vertex) - offset;
                        contacts.Add(new Contact(other.body, null, vertex + normal * dist, normal, dist));
                    }
                }

                return overlap;
        }

        return false;
    }
}
