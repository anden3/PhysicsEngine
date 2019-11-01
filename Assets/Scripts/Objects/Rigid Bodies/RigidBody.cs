using UnityEngine;

public class RigidBody : MonoBehaviour
{
	[Header("Physical Properties")]
	public float mass;
	public float inverseMass { get; protected set; }

    public bool affectedByGravity = true;

	[Range(0.0f, 1.0f)]
	public float linearDamping;
	[Range(0.0f, 1.0f)]
	public float angularDamping;

	[Header("Starting Conditions")]
	public Vector3 velocity;
	public Vector3 angularVelocity;
    public Vector3 acceleration;

	// Accumulators
	protected Vector3 forceAccum;
	protected Vector3 torqueAccum;

	// Matrices
	protected Matrix4x4 transformMatrix;

    [HideInInspector]
    public Matrix3x3 inverseInertiaTensor;

    [HideInInspector]
	public Matrix3x3 inverseInertiaTensorWorld = Matrix3x3.Identity;

    [HideInInspector]
    public BoundingVolume volume;

    [HideInInspector]
    public bool isStationary = false;

    protected bool isAwake = true;

	public bool HasFiniteMass() => inverseMass > 0.0f;

	protected virtual void Awake()
	{
		inverseMass = 1.0f / mass;
        volume = GetComponent<BoundingVolume>();

        switch (volume.type)
        {
            case BoundingVolume.Type.Cube:
                SetInertiaTensor(Matrix3x3.InertiaTensors.Cuboid(mass, ((BoundingCube)volume).bounds.extents));
                break;

            case BoundingVolume.Type.Sphere:
                SetInertiaTensor(Matrix3x3.InertiaTensors.Sphere(mass, ((BoundingSphere)volume).radius));
                break;
        }

        CalculateDerivedData();
    }

    private void OnEnable()
    {
        FindObjectOfType<RigidBodyPhysicsEngine>().Register(this);
    }
    private void OnDisable()
    {
        FindObjectOfType<RigidBodyPhysicsEngine>()?.Unregister(this);
    }

    public void SetInertiaTensor(Matrix3x3 inertiaTensor)
		=> inverseInertiaTensor = inertiaTensor.GetInverse();

	public void AddForce(Vector3 force)
	{
		forceAccum += force;
		isAwake = true;
	}

	/*
	 * Adds given force to given point on the rigid body.
	 * Both are given in world space.
	 */
	public void AddForceAtPoint(Vector3 force, Vector3 point)
	{
		Vector3 pt = point - transform.position;

		forceAccum += force;
		torqueAccum += Vector3.Cross(pt, force);

		isAwake = true;
	}

	/*
	 * Adds given force to given point on the rigid body.
	 * Force is given in world space, but point is given in model space.
	 */
	public void AddForceAtBodyPoint(Vector3 force, Vector3 point)
	{
		Vector3 pt = transform.TransformPoint(point);
		AddForceAtPoint(force, pt);
	}

    public Vector3 GetVelocityAtPoint(Vector3 point)
        => velocity + Vector3.Cross(angularVelocity, point - transform.position);

    public Vector3 GetVelocityAtBodyPoint(Vector3 point)
        => velocity + Vector3.Cross(angularVelocity, point);

	public bool Integrate(float deltaTime)
	{
		Vector3 linearAcc = acceleration + forceAccum * inverseMass;
		Vector3 angularAcc = inverseInertiaTensorWorld.Transform(torqueAccum);

		velocity += linearAcc * deltaTime;
		angularVelocity = angularAcc * deltaTime;

		// Add drag.
		velocity *= Mathf.Pow(linearDamping, deltaTime);
		angularVelocity *= Mathf.Pow(angularDamping, deltaTime);

        Vector3 prevPos = transform.position;

		transform.position += velocity * deltaTime;

		transform.rotation = Quaternion.AngleAxis(
			angularVelocity.magnitude * Time.deltaTime, angularVelocity
		) * transform.rotation;

		/*
		// Add drag.
		velocity *= Mathf.Pow(linearDamping, duration);
		angularVelocity *= Mathf.Pow(angularDamping, duration);
		*/

		CalculateDerivedData();
		ClearAccumulators();

        isStationary = transform.position == prevPos;
        return !isStationary;
	}

	protected void ClearAccumulators()
	{
		forceAccum = Vector3.zero;
		torqueAccum = Vector3.zero;
	}

	public void CalculateDerivedData()
	{
		// TODO: Might not be necessary if Unity does this automagically.
		transform.rotation.Normalize();

		transformMatrix.SetTRS(transform.position, transform.localRotation, Vector3.one);

		_transformInertiaTensor(
			ref inverseInertiaTensorWorld,
			inverseInertiaTensor, transformMatrix);
	}

	/*
	 * Transform inertia tensor from model space to world space.
	 */ 
	private static void _transformInertiaTensor(
		ref Matrix3x3 iitWorld, Matrix3x3 iitBody, Matrix4x4 rotmat)
	{
		float t4 = rotmat[0]  * iitBody[0] + rotmat[1] * iitBody[3] + rotmat[2]  * iitBody[6];
		float t9 = rotmat[0]  * iitBody[1] + rotmat[1] * iitBody[4] + rotmat[2]  * iitBody[7];
		float t14 = rotmat[0] * iitBody[2] + rotmat[1] * iitBody[5] + rotmat[2]  * iitBody[8];
		float t28 = rotmat[4] * iitBody[0] + rotmat[5] * iitBody[3] + rotmat[6]  * iitBody[6];
		float t33 = rotmat[4] * iitBody[1] + rotmat[5] * iitBody[4] + rotmat[6]  * iitBody[7];
		float t38 = rotmat[4] * iitBody[2] + rotmat[5] * iitBody[5] + rotmat[6]  * iitBody[8];
		float t52 = rotmat[8] * iitBody[0] + rotmat[9] * iitBody[3] + rotmat[10] * iitBody[6];
		float t57 = rotmat[8] * iitBody[1] + rotmat[9] * iitBody[4] + rotmat[10] * iitBody[7];
		float t62 = rotmat[8] * iitBody[2] + rotmat[9] * iitBody[5] + rotmat[10] * iitBody[8];

		iitWorld[0] = t4  * rotmat[0] + t9  * rotmat[1] + t14 * rotmat[2];
		iitWorld[1] = t4  * rotmat[4] + t9  * rotmat[5] + t14 * rotmat[6];
		iitWorld[2] = t4  * rotmat[8] + t9  * rotmat[9] + t14 * rotmat[10];
		iitWorld[3] = t28 * rotmat[0] + t33 * rotmat[1] + t38 * rotmat[2];
		iitWorld[4] = t28 * rotmat[4] + t33 * rotmat[5] + t38 * rotmat[6];
		iitWorld[5] = t28 * rotmat[8] + t33 * rotmat[9] + t38 * rotmat[10];
		iitWorld[6] = t52 * rotmat[0] + t57 * rotmat[1] + t62 * rotmat[2];
		iitWorld[7] = t52 * rotmat[4] + t57 * rotmat[5] + t62 * rotmat[6];
		iitWorld[8] = t52 * rotmat[8] + t57 * rotmat[9] + t62 * rotmat[10];
	}
}
