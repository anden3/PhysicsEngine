using UnityEngine;

using System.Collections.Generic;

public class Primitive : MonoBehaviour
{
    [HideInInspector]
    public RigidBody body;

    [HideInInspector]
    public bool isStationary = false;

    public bool HasBody() => body != null;

    private void OnEnable()
    {
        FindObjectOfType<RigidBodyPhysicsEngine>().Register(this);
    }
    private void OnDisable()
    {
        FindObjectOfType<RigidBodyPhysicsEngine>()?.Unregister(this);
    }

    public virtual bool GetContacts(Primitive other, List<Contact> contacts)
    {
        // No collision between stationary objects or objects without bodies,
        // for example two planes.
        if (isStationary && other.isStationary ||
            !HasBody() && !other.HasBody())
        {
            return false;
        }

        // Possible collision.
        return true;
    }
}