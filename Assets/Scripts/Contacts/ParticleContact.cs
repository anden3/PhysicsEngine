using UnityEngine;

public class ParticleContact
{
	public Particle[] particle = new Particle[2];

	public float restitution;
	public float penetration;
	public Vector3 contactNormal;

	public void Resolve(float duration)
	{
		ResolveVelocity(duration);
		ResolveInterpenetration(duration);
	}

	public float CalculateSeparatingVelocity()
	{
		Vector3 relativeVelocity = particle[0].velocity;
		if (particle[1]) relativeVelocity -= particle[1].velocity;

		return Vector3.Dot(relativeVelocity, contactNormal);
	}

	private void ResolveVelocity(float duration)
	{
		float separatingVelocity = CalculateSeparatingVelocity();

		if (separatingVelocity > 0)
		{
			// Particles are moving away from each other.
			return;
		}

		float newSepVelocity = -separatingVelocity * restitution;

		// Check the velocity caused by acceleration.
		Vector3 accCausedVelocity = particle[0].acceleration;
		if (particle[1]) accCausedVelocity -= particle[1].acceleration;
		float accCausedSepVelocity = Vector3.Dot(accCausedVelocity, contactNormal) * duration;

		if (accCausedSepVelocity < 0)
		{
			// Keep objects in resting contact.
			newSepVelocity += restitution * accCausedSepVelocity;
			if (newSepVelocity < 0) newSepVelocity = 0;
		}

		float deltaVelocity = newSepVelocity - separatingVelocity;

		float totalInverseMass = GetTotalIMass();

		// Continue if any of the particles have finite masses.
		if (totalInverseMass <= 0) return;

		// Calculate impulse.
		float impulse = deltaVelocity / totalInverseMass;

		// Get impulse per unit of inverse mass.
		Vector3 impulsePerIMass = contactNormal * impulse;

		// Apply impulses
		particle[0].velocity += impulsePerIMass * particle[0].inverseMass;

		if (particle[1])
		{
			// Opposite direction.
			particle[1].velocity += impulsePerIMass * -particle[1].inverseMass;
		}
	}

	private void ResolveInterpenetration(float duration)
	{
		if (penetration <= 0) return;

		float totalInverseMass = GetTotalIMass();

		// Continue if any of the particles have finite masses.
		if (totalInverseMass <= 0) return;

		// Find the amount of penetration resolution per unit of inverse mass.
		Vector3 movePerIMass = contactNormal * (penetration / totalInverseMass);

		particle[0].transform.localPosition
			+= movePerIMass * particle[0].inverseMass;

		if (particle[1])
		{
			particle[1].transform.localPosition
				+= movePerIMass * -particle[1].inverseMass;
		}
	}

	private float GetTotalIMass()
	{
		float totalInverseMass = particle[0].inverseMass;
		if (particle[1]) totalInverseMass += particle[1].inverseMass;

		return totalInverseMass;
	}
}
