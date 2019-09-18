/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

[RequireComponent(typeof(ParticleContactResolver))]
public class ParticleWorld : MonoBehaviour
{
	public Vector3 gravity = new Vector3(0.0f, -9.82f, 0.0f);

	public bool calculateIterations;

	protected Particle[] particles;

	private bool simulating;

	private ParticleContactResolver resolver;
	private List<ParticleContact> contacts = new List<ParticleContact>();

	public void StartSimulation() => simulating = true;

	private void Awake()
	{
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

		// Integrate particles.
		Integrate(Time.fixedDeltaTime);

		// Generate contacts.
		if (GenerateContacts() > 0)
		{
            // Go with a dynamic iteration count instead of fixed.
			if (calculateIterations)
				resolver.iterations = contacts.Count * 2;

			resolver.ResolveContacts(contacts, Time.fixedDeltaTime);
		}
	}

	private int GenerateContacts()
	{
		contacts.Clear();

        // Accumulate contacts.
		foreach (Particle p in particles)
		{
			p.GetContacts(ref contacts);
		}

		// Return number of contacts used.
		return contacts.Count;
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
