using UnityEngine;

using System.Collections.Generic;

using NaughtyAttributes;

[AddComponentMenu("Rigid Body Physics/Rigid Body")]
public class RigidBody : Primitive
{
	[Header("Physical Properties")]
	public float mass;

    [ShowNativeProperty]
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

    [Header("Sleep Settings")]
    public bool canSleep;

    [ShowIf("canSleep")] public bool isAwake;
    [ShowIf("canSleep")] public float motionBias;
    [ShowIf("canSleep")] public float sleepEpsilon;

    [ReadOnly] public float motion;

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
    public Vector3 lastFrameAcceleration;

	public bool HasFiniteMass() => inverseMass > 0.0f;

	protected virtual void Awake()
	{
        body = this;

        if (mass <= 0)
            inverseMass = 0;
        else
            inverseMass = 1.0f / mass;

        if (!canSleep)
            isAwake = true;

        volume = GetComponent<BoundingVolume>();

		Matrix3x3 inertiaTensor;

        switch (volume.type)
        {
            case BoundingVolume.Type.Cube:
				inertiaTensor = Matrix3x3.InertiaTensors.Cuboid(mass, ((BoundingCube)volume).bounds.extents);
                break;

            case BoundingVolume.Type.Sphere:
                inertiaTensor = Matrix3x3.InertiaTensors.Sphere(mass, ((BoundingSphere)volume).radius);
                break;

			default:
				throw new System.NotSupportedException(
					$"No inertia tensor found for the given bounding volume: {volume.type}.");
        }

		inverseInertiaTensor = inertiaTensor.GetInverse();
        CalculateDerivedData();
    }

	/// <summary>
	/// Set the awake state of the rigid body.
	/// </summary>
	/// <param name="awake">If the rigid body should be awake or not.</param>
    public void SetAwake(bool awake)
    {
        isAwake = awake;

        if (awake)
        {
            motion = sleepEpsilon * 2.0f;
        }
        else
        {
            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;
        }
    }

	/// <summary>
	/// Add linear <paramref name="force"/> to the rigid body.
	/// </summary>
	/// <param name="force">Force to apply to the center of mass.</param>
	public void AddForce(Vector3 force)
	{
		forceAccum += force;
		isAwake = true;
	}

    /// <summary>
    /// Adds given <paramref name="force"/> to given <paramref name="point"/> on the rigid body.
    /// </summary>
    /// <param name="force">Force in world space.</param>
    /// <param name="point">Point in world space.</param>
    public void AddForceAtPoint(Vector3 force, Vector3 point)
	{
		Vector3 pt = point - transform.position;

		forceAccum += force;
		torqueAccum += Vector3.Cross(pt, force);

		isAwake = true;
	}

    /// <summary>
    /// Adds given <paramref name="force"/> to given <paramref name="point"/> on the rigid body.
    /// </summary>
    /// <param name="force">Force in world space.</param>
    /// <param name="point">Point in model space.</param>
    public void AddForceAtBodyPoint(Vector3 force, Vector3 point)
	{
		Vector3 pt = transform.TransformPoint(point);
		AddForceAtPoint(force, pt);
	}

	/// <summary>
	/// Get velocity of a <paramref name="point"/>.
	/// </summary>
	/// <param name="point">A point in world space.</param>
	/// <returns>Velocity relative to world.</returns>
    public Vector3 GetVelocityAtPoint(Vector3 point)
	{
		return GetLinearVelocity() + Vector3.Cross(angularVelocity, point - transform.position);
	}

    /// <summary>
    /// Get velocity of a <paramref name="point"/>.
    /// </summary>
    /// <param name="point">A point in model space.</param>
    /// <returns>Velocity relative to world.</returns>
    public Vector3 GetVelocityAtBodyPoint(Vector3 point)
	{
		return GetLinearVelocity() + Vector3.Cross(angularVelocity, point);
	}

	/// <summary>
	/// Set the <paramref name="position"/> of the rigid body.
	/// </summary>
	/// <param name="position">The new position.</param>
	public void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

	public void ChangeVelocity(Vector3 deltaV)
	{
        velocity += deltaV;
    }

	public Vector3 GetLinearVelocity() {
        return velocity;
    }

	public bool Integrate(float deltaTime)
	{
        if (!isAwake)
            return false;

        Vector3 linearAcc = acceleration + forceAccum * inverseMass;
		Vector3 angularAcc = inverseInertiaTensorWorld.Transform(torqueAccum);
        lastFrameAcceleration = linearAcc;

        Vector3 lastPosition = transform.position;
        transform.position += velocity * deltaTime;

        velocity += linearAcc * deltaTime;
        velocity *= Mathf.Pow(linearDamping, deltaTime);

        angularVelocity = angularAcc * deltaTime;
        angularVelocity *= Mathf.Pow(angularDamping, deltaTime);

        transform.rotation = Quaternion.AngleAxis(
            angularVelocity.magnitude * Time.deltaTime, angularVelocity
        ) * transform.rotation;

        CalculateDerivedData();

        if (canSleep)
        {
            float currentMotion = GetLinearVelocity().sqrMagnitude + angularVelocity.sqrMagnitude;
            motion = Mathf.Clamp(motionBias * motion + (1 - motionBias) * currentMotion, 0, sleepEpsilon * 10);

            if (motion < sleepEpsilon)
                SetAwake(false);
        }
		
		ClearAccumulators();

        isStationary = transform.position == lastPosition;
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

    public override bool GetContacts(Primitive other, List<Contact> contacts)
    {
        if (!base.GetContacts(other, contacts))
            return false;

        // RigidBody-Plane collision. Let the Plane class handle this.
        if (!other.HasBody())
            return other.GetContacts(this, contacts);

        // RigidBody-RigidBody collision.
        return volume.GetContacts(other.body.volume, contacts);
    }

    /*
	 * Transform inertia tensor from model space to world space.
	 */
    private static void _transformInertiaTensor(
		ref Matrix3x3 iitWorld, Matrix3x3 iitBody, Matrix4x4 rotmat)
	{
		float t4  = rotmat[0] * iitBody[0] + rotmat[1] * iitBody[3] + rotmat[2]  * iitBody[6];
		float t9  = rotmat[0] * iitBody[1] + rotmat[1] * iitBody[4] + rotmat[2]  * iitBody[7];
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
