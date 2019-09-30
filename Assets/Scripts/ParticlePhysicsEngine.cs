/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

[RequireComponent(
    typeof(ParticleForceRegistry)
)]
[AddComponentMenu("Particle Physics/Particle Physics Engine")]
public class ParticlePhysicsEngine : MonoBehaviour
{
    private ParticleForceRegistry registry;

    private static List<Particle> particles = new List<Particle>();

    public static void Register(Particle p) => particles.Add(p);
    public static void Unregister(Particle p) => particles.Remove(p);

    private void Awake()
	{
        registry = GetComponent<ParticleForceRegistry>();
	}

    private void FixedUpdate()
	{
        // Update all force generators.
        registry.UpdateForces(Time.fixedDeltaTime);

		// Integrate particles.
		Integrate(Time.fixedDeltaTime);
    }

	private void Integrate(float duration)
	{
        foreach (Particle p in particles)
		{
			p.Integrate(duration);
		}
	}

    public void SetDamping(float damping)
    {
        foreach (Particle p in particles)
        {
            p.damping = damping;
        }
    }

    public void ResetSimulation()
    {
        foreach (Particle p in particles)
        {
            p.ParticleReset();
        }
    }
}
