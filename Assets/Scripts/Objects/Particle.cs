/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

public class Particle : MonoBehaviour
{
	[Header("Physical Properties")]
	public float mass = 1;

	[Range(0.0f, 1.0f)]
	public float bounciness = 0.5f;
	[Range(0.0f, 1.0f)]
	public float damping = 0.5f;
    [Range(0.0f, 1.0f)]
    public float groundDamping = 0.5f;

    private Vector3 startPos;

	public float inverseMass { get; protected set; }
    public Vector3 position
    {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }

	[Header("Starting Conditions")]
	public Vector3 velocity;
	public Vector3 acceleration { get; set; }

	protected Vector3 forceAccum;

    protected bool onGround;

	public bool HasFiniteMass() => inverseMass > 0.0f;

	protected virtual void Awake()
	{
		inverseMass = 1.0f / mass;
        startPos = transform.position;

    }

    private void Start() => ParticlePhysicsEngine.Register(this);
    private void OnDestroy() => ParticlePhysicsEngine.Unregister(this);

    public void AddForce(in Vector3 force) => forceAccum += force;
    protected void ClearAccumulator() => forceAccum = Vector3.zero;

    public void Integrate(float duration)
	{
		Debug.Assert(duration > 0.0f);

		// Only accept finite masses.
		if (!HasFiniteMass()) return;

		// Update position.
		transform.position += velocity * duration;

		// Get acceleration from forces.
		Vector3 resultingAcc = acceleration;
		resultingAcc += forceAccum * inverseMass;

		// Update velocity.
		velocity += resultingAcc * duration;

		// Add drag.
		velocity *= Mathf.Pow(1 - damping, duration);

        if (onGround)
        {
            velocity *= Mathf.Pow(groundDamping, duration);
        }

		// Clear forces.
		ClearAccumulator();
    }

    public void ParticleReset()
    {
        transform.position = startPos;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        forceAccum = Vector3.zero;
    }
}
