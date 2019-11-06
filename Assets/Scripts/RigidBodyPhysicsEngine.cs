/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 */

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
    public Vector3 gravity = new Vector3(0.0f, -9.82f, 0.0f);
    [Space]
    public Bounds playArea;

    [Header("Contact Settings")]
    [Range(0, 1)] public float friction = 0.5f;
    [Range(0, 1)] public float restitution = 0.2f;

    public float angularLimit = 0.2f;
    public float velocityLimit = 0.25f;

    private Octree octree;
    private readonly List<RigidBody> bodies = new List<RigidBody>();

    private ContactResolver resolver;
    private readonly List<Contact> contacts = new List<Contact>();

    public void Register(Primitive p)
    {
        if (p.HasBody())
        {
            if (p.body.affectedByGravity)
                p.body.acceleration = gravity;

            bodies.Add((RigidBody)p);
        }

        if (octree == null)
            Octree.Enqueue_Static(p);
        else
            octree.Enqueue(p);
        
    }
    public void Unregister(Primitive p)
    {
        if (p.HasBody())
            bodies.Remove((RigidBody)p);
    }

    private void OnValidate()
    {
        Octree.FindEnclosingCube(ref playArea);
    }

    private void Awake()
    {
        Contact.friction = friction;
        Contact.restitution = restitution;
        Contact.angularLimit = angularLimit;
        Contact.velocityLimit = velocityLimit;

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
