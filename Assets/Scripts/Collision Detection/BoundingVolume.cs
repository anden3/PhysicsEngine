using UnityEngine;

public abstract class BoundingVolume : MonoBehaviour
{
	public virtual bool Overlaps(BoundingVolume other) => false;
	public virtual float GetSize() => 0;
}
