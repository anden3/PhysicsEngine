using UnityEngine;

[AddComponentMenu("Particle Physics/Force Generators/Gravity")]
public class ParticleGravity : ParticleForceGenerator
{
	public Vector3 gravity;

	public override void UpdateForce(float deltaTime)
	{
		if (!particle.HasFiniteMass()) return;

		particle.AddForce(gravity * particle.mass);
	}
}
