using UnityEngine;
using UnityEngine.InputSystem;

using NaughtyAttributes;

public class Cannon : MonoBehaviour
{
    private const float MAX_SPEED = 100.0f;

    [ValidateInput("IsRigidBody", "Object needs to be a Rigid Body.")]
    [ShowAssetPreview]
    public GameObject item;

    [Header("Timings")]
    // public float spawnDelay;
    public bool itemDecay;
    [EnableIf("itemDecay")] public float lifetime;
    
    [Header("Firing Settings")]
    [Slider(0.0f, 90.0f)]
    public float startElevation;

    [Slider(0.0f, 90.0f)]
    public float startTraverse;

    [Slider(0.0f, MAX_SPEED)]
    public float firingSpeed;

    [Header("Visualization")]
    public Color trajectoryColor;
    public int maxIterations = 1000;

    [Header("Key Bindings")]
    public InputAction aimAction;
    public InputAction fireAction;
    public InputAction powerAction;

    public float Elevation
    {
        get => -cannonTube.eulerAngles.z;
        set => SetElevation(value);
    }

    public float Traverse
    {
        get => transform.eulerAngles.y;
        set => SetTraverse(value);
    }

    public float Power
    {
        get => firingSpeed;
        set => firingSpeed = Mathf.Clamp(value, 0.0f, MAX_SPEED);
    }

    [HideInInspector]
    public RigidBody body;

    private RigidBodyPhysicsEngine engine;
    private Plane[] planes;

    private Transform cannonTube;
    private Transform cannonNozzle;

    private void OnValidate()
    {
        cannonTube = transform.Find("CannonTube");
        cannonNozzle = cannonTube.Find("CannonNozzle");

        Elevation = startElevation;
        Traverse = startTraverse;
    }

    private void Awake()
    {
        if (item)
            body = item.GetComponent<RigidBody>();

        engine = FindObjectOfType<RigidBodyPhysicsEngine>();
        planes = FindObjectsOfType<Plane>();

        fireAction.performed += OnFire;
    }

    private void OnEnable()
    {
        aimAction.Enable();
        fireAction.Enable();
        powerAction.Enable();
    }

    private void OnDisable()
    {
        aimAction.Disable();
        fireAction.Disable();
        powerAction.Disable();
    }

    private void Update()
    {
        Aim(aimAction.ReadValue<Vector2>());

        Power += powerAction.ReadValue<float>();
    }

    public void SetElevation(float angle)
    {
        Vector3 angles = cannonTube.eulerAngles;
        angles.z = -Mathf.Clamp(angle, 0.0f, 90.0f);
        cannonTube.eulerAngles = angles;
    }

    public void SetTraverse(float angle)
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = Mathf.Clamp(angle, 0.0f, 90.0f);
        transform.eulerAngles = angles;
    }

    private bool IsRigidBody(GameObject obj)
        => obj && obj.TryGetComponent(out RigidBody rb);


    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !body)
            return;
        
        Gizmos.color = trajectoryColor;
        DrawTrajectory();
    }

    private void DrawTrajectory()
    {
        int iterations = 0;
        float currentTime = 0.0f;
        Vector3 currentPosition = cannonNozzle.position;

        Vector3 acceleration = body.affectedByGravity ? engine.gravity : Vector3.zero;
        Vector3 velocity = cannonNozzle.forward * firingSpeed;

        while (iterations < maxIterations &&
              (!itemDecay || currentTime <= lifetime) &&
              !CheckPlaneIntersection(currentPosition))
        {
            Vector3 lastPosition = currentPosition;
            currentPosition += velocity * Time.fixedDeltaTime;

            velocity += acceleration * Time.fixedDeltaTime;
            velocity *= Mathf.Pow(body.linearDamping, Time.fixedDeltaTime);

            Gizmos.DrawLine(lastPosition, currentPosition);
            currentTime += Time.fixedDeltaTime;
            iterations++;
        }

        if (iterations < maxIterations && (!itemDecay || currentTime <= lifetime))
        {
            Gizmos.DrawSphere(currentPosition, 0.3f);
        }
    }

    private bool CheckPlaneIntersection(Vector3 position)
    {
        foreach (Plane p in planes)
        {
            Vector3 relativePosition = position - p.transform.position;

            if (Vector3.Dot(relativePosition, p.normal) <= 0)
                return true;
        }

        return false;
    }

    public void Aim(Vector2 aim)
    {
        Traverse += aim.x;

        startElevation = Mathf.Clamp(startElevation + aim.y, 0.0f, 90.0f);
        Elevation = startElevation;
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (engine.state != RigidBodyPhysicsEngine.SimState.Running)
            return;

        RigidBody rb = Instantiate(item, cannonNozzle.position, Quaternion.identity, transform.parent).GetComponent<RigidBody>();
        rb.ChangeVelocity(cannonNozzle.forward * firingSpeed);

        if (itemDecay)
            Destroy(rb.gameObject, lifetime);
    }
}
