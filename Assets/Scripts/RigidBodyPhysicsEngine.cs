/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 */

using UnityEngine;
using UnityEngine.InputSystem;

using System.Diagnostics;
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
    public bool useOctree = true;
    public Vector3 gravity = new Vector3(0.0f, -9.82f, 0.0f);
    [Space]
    public Bounds playArea;

    [Header("Contact Settings")]
    [Range(0, 1)] public float friction = 0.5f;
    [Range(0, 1)] public float restitution = 0.2f;

    public float angularLimit = 0.2f;
    public float velocityLimit = 0.25f;

    [Header("Key Bindings")]
    public InputAction toggleSimulationAction;
    public InputAction toggleOctreeGizmoAction;

    private Octree octree;
    private bool drawingOctree = false;

    private ContactResolver resolver;
    private readonly List<Contact> contacts = new List<Contact>();

    private Stopwatch stopwatch = new Stopwatch();

    public void Register(Primitive p)
    {
        if (p.HasBody())
        {
            if (p.body.affectedByGravity)
                p.body.acceleration = gravity;
        }

        if (octree == null)
            Octree.Enqueue_Static(p);
        else
            octree.Enqueue(p);
        
    }
    public void Unregister(Primitive p) {}

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

        Octree.minSize = useOctree ? Vector3.one : playArea.size;

        octree = new Octree(playArea);
        resolver = GetComponent<ContactResolver>();
    }

    private void Start()
    {
        toggleSimulationAction.performed += (ctx) =>
        {
            if (state == SimState.Running)
                state = SimState.Stopped;
            else
                state = SimState.Running;
        };

        toggleOctreeGizmoAction.performed += (ctx)
            => drawingOctree = !drawingOctree;
    }

    private void OnEnable()
    {
        toggleSimulationAction.Enable();
        toggleOctreeGizmoAction.Enable();
    }

    private void OnDisable()
    {
        toggleSimulationAction.Disable();
        toggleOctreeGizmoAction.Disable();
    }

    private void FixedUpdate()
	{
        if (state != SimState.Running)
            return;

        stopwatch.Start();
        octree.Update(Time.fixedDeltaTime);
        stopwatch.Stop();

        System.TimeSpan updateDuration = stopwatch.Elapsed;
        stopwatch.Reset();

        stopwatch.Start();
        octree.GetContacts(contacts);
        stopwatch.Stop();

        System.TimeSpan contactDetectionDuration = stopwatch.Elapsed;
        stopwatch.Reset();

        resolver.ResolveContacts(contacts, Time.fixedDeltaTime);
        contacts.Clear();

        /*
        Recorder.LogData(
            $"update_octree_{(useOctree ? "enabled" : "disabled")}",
            Octree.objectCount, updateDuration.TotalMilliseconds
        );

        Recorder.LogData(
            $"detect_octree_{(useOctree ? "enabled" : "disabled")}",
            Octree.objectCount, contactDetectionDuration.TotalMilliseconds
        );
        */
    }

    private void OnDrawGizmos()
    {
        if (drawingOctree)
            Octree.Debug_Draw();
        else
        {
            Gizmos.color = (state == SimState.Running) ? Color.red : Color.green;
            Gizmos.DrawWireCube(playArea.center + transform.localPosition, playArea.size);
        }
    }
}
