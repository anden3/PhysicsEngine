using UnityEngine;

using System.Linq;
using System.Collections.Generic;

[AddComponentMenu("Particle Physics/Force Generators/Gravity")]
[RequireComponent(typeof(Particle))]
public class ParticleGravity : ParticleForceGenerator
{
    public static float G = 6.67e3f;

    private List<Particle> particles;

    private void Start()
    {
        particles = FindObjectsOfType<Particle>().ToList();

        // Don't include yourself in the gravity calculations.
        particles.Remove(particle);
    }

    public override void UpdateForce(float deltaTime)
	{
		if (!particle.HasFiniteMass()) return;

        Vector3 gravForces = Vector3.zero;

        foreach (Particle body in particles)
        {
            Vector3 gravVec = body.position - particle.position;

            gravForces += gravVec.normalized * (G * (particle.mass * body.mass) / gravVec.sqrMagnitude);
        }

        particle.AddForce(gravForces / 1e28f);
	}
}
