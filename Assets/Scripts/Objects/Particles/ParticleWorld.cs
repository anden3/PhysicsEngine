/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

[RequireComponent(typeof(ParticleContactResolver))]
public class ParticleWorld : MonoBehaviour
{
	public Vector3 gravity = new Vector3(0.0f, -9.82f, 0.0f);

	public bool calculateIterations;

    [Header("UI Components")]
    public InputField gravityField;
    public InputField massField;

    public Slider airResistanceSlider;
    public Slider groundFrictionSlider;
    public Slider bouncinessSlider;

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

        Debug.Assert(particles.Length >= 1);
        Particle particle = particles[0];

        gravityField.text = (-gravity.y).ToString();
        massField.text = particle.mass.ToString();

        airResistanceSlider.value = 1.0f - particle.airResistanceCoef;
        groundFrictionSlider.value = 1.0f - particle.groundFrictionCoef;
        bouncinessSlider.value = particle.bounciness;
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

	private void Integrate(float deltaTime)
	{
		foreach (Particle p in particles)
		{
			p.Integrate(deltaTime);
		}
	}

    public void Reset()
    {
        simulating = false;

        foreach (Particle p in particles)
        {
            p.Reset();
        }
    }

    public void SetGravity(string value)
    {
        Vector3 newG = Vector3.down * float.Parse(value) * -1;
        Vector3 deltaG = newG - gravity;

        foreach (Particle p in particles)
        {
            p.acceleration += deltaG;
        }

        gravity = newG;
    }

    public void SetMass(string value)
    {
        foreach (Particle p in particles)
        {
            p.SetMass(float.Parse(value));
        }
    }

    public void SetAirResistance(float value)
    {
        foreach (Particle p in particles)
        {
            p.airResistanceCoef = 1.0f - value;
        }
    }

    public void SetGroundFriction(float value)
    {
        foreach (Particle p in particles)
        {
            p.groundFrictionCoef = 1.0f - value;
        }
    }

    public void SetBounciness(float value)
    {
        foreach (Particle p in particles)
        {
            p.bounciness = value;
        }
    }
}
