/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

public class ParticleContact
{
    // Used to maintain resting contact.
    public const float GROUND_DAMPING = 4.0f;

	public Particle first;
	public Particle second;

	public float restitution;
	public float penetration;
	public Vector3 contactNormal;

	public void Resolve(float deltaTime)
	{
		ResolveVelocity(deltaTime);
		ResolveInterpenetration(deltaTime);
	}

	/*
	 * Calculate how quickly the particles move away from each other,
	 * or towards each other if negative.
	 */
	public float CalculateSeparatingVelocity()
	{
		Vector3 relativeVelocity = first.velocity;
		if (second) relativeVelocity -= second.velocity;

		return Vector3.Dot(relativeVelocity, contactNormal);
	}

	private void ResolveVelocity(float deltaTime)
	{
		float separatingVelocity = CalculateSeparatingVelocity();

		if (separatingVelocity > 0)
		{
			// Particles are moving away from each other.
			return;
		}

		// Get velocity after having "bounced".
		float newSepVelocity = -separatingVelocity * restitution;

		// Check the velocity caused by acceleration.
		Vector3 accCausedVelocity = first.acceleration;
		if (second) accCausedVelocity -= second.acceleration;
		float accCausedSepVelocity = Vector3.Dot(accCausedVelocity, contactNormal) * deltaTime * GROUND_DAMPING;

		// If there's gravity causing the objects to move together.
		if (accCausedSepVelocity < 0)
		{
            // Keep objects in resting contact.
            newSepVelocity += restitution * accCausedSepVelocity;
			if (newSepVelocity < 0) newSepVelocity = 0;
		}

		// Get difference between current velocity and post-bounce velocity.
		float deltaVelocity = newSepVelocity - separatingVelocity;

		float totalInverseMass = GetTotalIMass();

		// Continue if any of the particles have finite masses.
		if (totalInverseMass <= 0) return;

		// Calculate impulse.
		float impulse = deltaVelocity / totalInverseMass;

		// Get impulse per unit of inverse mass.
		Vector3 impulsePerIMass = contactNormal * impulse;

		// Apply impulses
		first.velocity += impulsePerIMass * first.inverseMass;

		if (second)
		{
			// Opposite direction.
			second.velocity += impulsePerIMass * -second.inverseMass;
		}
	}

	private void ResolveInterpenetration(float deltaTime)
	{
		if (penetration <= 0) return;

		float totalInverseMass = GetTotalIMass();

		// Continue if any of the particles have finite masses.
		if (totalInverseMass <= 0) return;

		// Find the amount of penetration resolution per unit of inverse mass.
		Vector3 movePerIMass = contactNormal * (penetration / totalInverseMass);

		first.transform.localPosition
			+= movePerIMass * first.inverseMass;

		if (second)
		{
			second.transform.localPosition
				+= movePerIMass * -second.inverseMass;
		}
	}

	private float GetTotalIMass()
	{
		float totalInverseMass = first.inverseMass;
		if (second) totalInverseMass += second.inverseMass;

		return totalInverseMass;
	}
}
