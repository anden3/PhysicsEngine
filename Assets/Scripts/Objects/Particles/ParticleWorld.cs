/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(
    typeof(ParticleContactResolver),
    typeof(ParticleForceRegistry)
)]
public class ParticleWorld : MonoBehaviour
{
	public Vector3 gravity = new Vector3(0.0f, -9.82f, 0.0f);

	protected Particle[] particles;

	private bool simulating;

    private ParticleForceRegistry registry;
    private ParticleContactResolver resolver;

	public void StartSimulation() => simulating = true;

	private void Awake()
	{
        registry = GetComponent<ParticleForceRegistry>();
		resolver = GetComponent<ParticleContactResolver>();

		particles = FindObjectsOfType<Particle>();
	}

    private void Start()
    {
        // Set gravity of all particles.
        foreach (Particle p in particles)
        {
            p.acceleration = gravity;
        }
    }

    private void FixedUpdate()
	{
		if (!simulating) return;

        // Update all force generators.
        registry.UpdateForces(Time.fixedDeltaTime);

		// Integrate particles.
		Integrate(Time.fixedDeltaTime);
        resolver.ResolveContacts(Time.fixedDeltaTime);
    }

	private void Integrate(float duration)
	{
        foreach (Particle p in particles)
		{
			p.Integrate(duration);
		}
	}

    public void Reset()
        => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
