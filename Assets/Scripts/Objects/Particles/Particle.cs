/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

public class Particle : MonoBehaviour, IParticleContactGenerator
{
    [Header("Physical Properties")]
    public float mass;

	public double inverseMass { get; protected set; }
    public Vector3 position
    {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }

	[Header("Starting Conditions")]
    public Vector3 velocity;
    public float secondsPerDay;
    public Vector3 rotationAxis = Vector3.up;

	public Vector3 acceleration { get; protected set; }

    // Initial values used when resetting particle.
    private Vector3 startPos;
    private Vector3 startVelocity;
    private Quaternion startRotation;

    protected Vector3 forceAccum;

	public bool HasFiniteMass() => inverseMass > 0.0f;

	protected virtual void Awake()
	{
		inverseMass = 1.0f / mass / UnitScales.Mass;

        startPos = transform.position;
        startVelocity = velocity;
        startRotation = transform.rotation;
    }

    private void Start() => ParticlePhysicsEngine.Register(this);
    private void OnDestroy() => ParticlePhysicsEngine.Unregister(this);

    public void AddForce(in Vector3 force) => forceAccum += force;
    protected void ClearAccumulator() => forceAccum = Vector3.zero;

    public void Integrate(float deltaTime)
	{
        if (secondsPerDay > 0)
        {
            transform.Rotate(
                rotationAxis, (-360.0f * deltaTime) / secondsPerDay, Space.Self
            );
        }

		if (!HasFiniteMass()) return;

		transform.position += velocity * deltaTime;

		Vector3 resultingAcc = acceleration;
		resultingAcc += forceAccum * (float)inverseMass;

		velocity += resultingAcc * deltaTime;

        ClearAccumulator();
    }

    public virtual void GetContacts(ref List<ParticleContact> contacts) { }

    public void ParticleReset()
    {
        transform.position = startPos;
        transform.rotation = startRotation;

        velocity = startVelocity;
        acceleration = Vector3.zero;
        forceAccum = Vector3.zero;

        if (TryGetComponent(out TrailRenderer trail))
        {
            trail.Clear();
        }
    }
}
