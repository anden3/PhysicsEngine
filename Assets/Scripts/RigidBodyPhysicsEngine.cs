using UnityEngine;

using System.Collections.Generic;

[AddComponentMenu("Rigid Body Physics/Rigid Body Physics Engine")]
public class RigidBodyPhysicsEngine : MonoBehaviour
{
    public enum SimState
    {
        Stopped,
        Running
    }

    [Header("Global Settings")]
    public SimState state;
    [Space]
    public Vector3 gravity = new Vector3(0.0f, -9.82f, 0.0f);
    [Space]
    public Bounds playArea;

    private Octree octree;
    private readonly List<RigidBody> bodies = new List<RigidBody>();

    private ContactResolver resolver;
    private readonly List<Contact> contacts = new List<Contact>();

    public void Register(RigidBody b)
    {
        if (b.affectedByGravity)
            b.acceleration = gravity;

        bodies.Add(b);

        if (octree == null)
            Octree.Enqueue_Static(b);
        else
            octree.Enqueue(b);
        
    }
    public void Unregister(RigidBody b)
    {
        bodies.Remove(b);
    }

    private void OnValidate()
    {
        Octree.FindEnclosingCube(ref playArea);
    }

    private void Awake()
    {
        octree = new Octree(playArea);
        resolver = GetComponent<ContactResolver>();
    }

    private void FixedUpdate()
	{
        if (state != SimState.Running)
            return;
        
        /*
		foreach (RigidBody body in bodies)
		{
			registry.UpdateForces(Time.fixedDeltaTime);
		}
        */

        octree.Update(Time.fixedDeltaTime);
        octree.GetContacts(contacts);

        resolver.ResolveContacts(contacts, Time.fixedDeltaTime);
        contacts.Clear();
	}

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            Octree.Debug_Draw();
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(playArea.center + transform.localPosition, playArea.size);
        }
    }
}
