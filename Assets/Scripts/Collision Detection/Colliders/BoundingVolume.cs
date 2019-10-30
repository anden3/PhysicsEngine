using UnityEngine;

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

    public abstract bool Overlaps(BoundingVolume other, out Contact contact);
	public abstract bool Overlaps(BoundingCube c, out Contact contact);
    public abstract bool Overlaps(BoundingSphere s, out Contact contact);

    public abstract bool IsPointInside(Vector3 point);
    public float GetGrowth(BoundingVolume newVolume)
        => newVolume.size - size;

    private void OnValidate()
    {
        if (TryGetComponent(out RigidBody body))
            this.body = body;
    }
}
