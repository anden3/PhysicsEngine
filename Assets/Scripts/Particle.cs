/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

public class Particle : MonoBehaviour
{
    [Header("Physical Properties")]
    public float mass;
	public double inverseMass { get; protected set; }

	[Header("Starting Conditions")]
    public Vector3 velocity;
    public float secondsPerDay;
    public Vector3 rotationAxis = Vector3.up;

    // Initial values used when resetting particle.
    private Vector3 startPos;
    private Vector3 startVelocity;
    private Quaternion startRotation;

    protected Vector3 forceAccum;

	protected virtual void Awake()
	{
		inverseMass = 1.0f / mass / UnitScales.Mass;

        startPos = transform.position;
        startVelocity = velocity;
        startRotation = transform.rotation;
    }

    private void Start() => ParticlePhysicsEngine.Register(this);
    private void OnDestroy() => ParticlePhysicsEngine.Unregister(this);

    public void AddForce(in Vector3 force)
        => forceAccum += force;

    public void Integrate(float deltaTime)
	{
        if (secondsPerDay > 0)
        {
            // Rotate counter-clockwise.
            transform.Rotate(
                rotationAxis, (-360.0f * deltaTime) / secondsPerDay, Space.Self
            );
        }

		transform.position += velocity * deltaTime;

        Vector3 acceleration = forceAccum * (float)inverseMass;
		velocity += acceleration * deltaTime;

        forceAccum = Vector3.zero;
    }

    public void ParticleReset()
    {
        transform.position = startPos;
        transform.rotation = startRotation;

        velocity = startVelocity;
        forceAccum = Vector3.zero;

        if (TryGetComponent(out TrailRenderer trail))
            trail.Clear();
    }
}
