using UnityEngine;

using System.Collections.Generic;

[RequireComponent(typeof(ParticleForceRegistry), typeof(ParticleContactResolver))]
public class ParticleWorld : MonoBehaviour
{
	public Vector3 gravity = new Vector3(0.0f, -9.82f, 0.0f);

	public bool calculateIterations;

	protected Particle[] particles;

	private ParticleForceRegistry registry;
	private ParticleContactResolver resolver;

	// private List<ParticleContactGenerator> contactGenerators;
	private List<ParticleContact> contacts = new List<ParticleContact>();

	private void Awake()
	{
		registry = GetComponent<ParticleForceRegistry>();
		resolver = GetComponent<ParticleContactResolver>();

		particles = FindObjectsOfType<Particle>();
		// TODO: contactGenerators = FindObjectsOfType<ParticleContactGenerator>();
	}

	private void FixedUpdate()
	{
		// Add gravity to all particles.
		foreach (Particle p in particles)
		{
			p.AddForce(gravity);
		}

		// Apply forces.
		registry.UpdateForces(Time.fixedDeltaTime);

		// Integrate particles.
		Integrate(Time.fixedDeltaTime);

		// Generate contacts.
		if (GenerateContacts() > 0)
		{
			if (calculateIterations) resolver.iterations = contacts.Count * 2;
			resolver.ResolveContacts(contacts, Time.fixedDeltaTime);
		}
	}

	private int GenerateContacts()
	{
		contacts.Clear();

		/*
		// Retrieve all new contacts from the generators.
		foreach (ParticleContactGenerator g in contactGenerators)
		{
			foreach (var c in g.GetContacts())
			{
				contacts.Add(c);
				if (--limit <= 0) goto limitReached;
			}

			if (limit <= 0) goto limitReached;
		}
		*/

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
}
