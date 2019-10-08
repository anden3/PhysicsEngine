using UnityEngine;

public abstract class BoundingVolume : MonoBehaviour
{
    public enum Type
    {
        Sphere,
        Cube,
        Capsule
    }

    public abstract Type type { get; }
    public abstract Vector3 center { get; }

	public abstract bool Overlaps(BoundingVolume other);
    public abstract bool IsPointInside(Vector3 point);
    public abstract float GetSize();
    public float GetGrowth(BoundingVolume newVolume)
        => newVolume.GetSize() - GetSize();
}
