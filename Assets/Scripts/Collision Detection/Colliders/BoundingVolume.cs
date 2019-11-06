using UnityEngine;

using System.Collections.Generic;

public abstract class BoundingVolume : MonoBehaviour
{
    public enum Type
    {
        Sphere,
        Cube
    }

    public abstract Type type { get; }
    public abstract Vector3 center { get; }
    public abstract float size { get; }

    public RigidBody body;

    public abstract bool GetContacts(BoundingVolume other, List<Contact> contacts);
	public abstract bool GetContacts(BoundingCube c, List<Contact> contacts);
    public abstract bool GetContacts(BoundingSphere s, List<Contact> contacts);

    public abstract bool IsPointInside(Vector3 point);
    public float GetGrowth(BoundingVolume newVolume)
        => newVolume.size - size;

    private void OnValidate()
    {
        if (TryGetComponent(out RigidBody body))
            this.body = body;
    }
}
