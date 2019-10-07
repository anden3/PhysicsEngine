/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

[RequireComponent(
    typeof(ParticleContactResolver),
    typeof(ParticleForceRegistry)
)]
[AddComponentMenu("Particle Physics/Particle Physics Engine")]
public class ParticlePhysicsEngine : MonoBehaviour
{
    [Header("Global Settings")]
    public float timeScale = 1.0f;
    public bool playOnAwake = true;

	private bool simulating;

    private ParticleForceRegistry registry;
    private ParticleContactResolver resolver;

    private static List<Particle> particles = new List<Particle>();

    public static void Register(Particle p)
    {
        particles.Add(p);
        ParticleContactResolver.Register(p);
    }
    public static void Unregister(Particle p)
    {
        particles.Remove(p);
        ParticleContactResolver.Unregister(p);
    }

	public void StartSimulation() => simulating = true;

	private void Awake()
	{
        registry = GetComponent<ParticleForceRegistry>();
		resolver = GetComponent<ParticleContactResolver>();

        simulating = playOnAwake;
	}

    private void Start()
    {
        /*
        // Set gravity of all particles.
        foreach (Particle p in particles)
        {
            p.acceleration = gravity;
        }
        */
    }

    private void FixedUpdate()
	{
		if (!simulating) return;

        // Update all force generators.
        registry.UpdateForces(Time.fixedDeltaTime * timeScale);

		// Integrate particles.
		Integrate(Time.fixedDeltaTime * timeScale);
        resolver.ResolveContacts(Time.fixedDeltaTime * timeScale);
    }

	private void Integrate(float duration)
	{
        foreach (Particle p in particles)
		{
			p.Integrate(duration);
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
