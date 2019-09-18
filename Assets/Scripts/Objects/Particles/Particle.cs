using UnityEngine;

using System.Collections.Generic;

public class Particle : MonoBehaviour
{
	[Header("Physical Properties")]
	public float mass;

	[Range(0.0f, 1.0f)]
	public float bounciness;
	[Range(0.0f, 1.0f)]
	public float damping;

	public float inverseMass { get; protected set; }

	[Header("Starting Conditions")]
	public Vector3 velocity;
	public Vector3 acceleration { get; protected set; }

	protected Vector3 forceAccum;

	public bool HasFiniteMass() => inverseMass > 0.0f;

	protected virtual void Awake()
	{
		inverseMass = 1.0f / mass;
	}

	public void AddForce(in Vector3 force)
	{
		forceAccum += force;
	}

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
		velocity *= Mathf.Pow(damping, duration);

		// Clear forces.
		ClearAccumulator();
	}

	protected void ClearAccumulator()
	{
		forceAccum = Vector3.zero;
	}

	public virtual void GetContacts(ref List<ParticleContact> contacts) { }
}
