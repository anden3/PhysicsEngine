using UnityEngine;
using System.Collections.Generic;

// Holds all the force generators and the particles that they apply to.
public class ParticleForceRegistry : MonoBehaviour
{
	public static ParticleForceRegistry registry = null;

    protected struct ParticleForceRegistration
	{
		public Particle particle;
		public ParticleForceGenerator fg;
	}

	protected List<ParticleForceRegistration> registrations = new List<ParticleForceRegistration>();

	private void Awake()
	{
		if (registry == null)
		{
			registry = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void Add(Particle particle, ParticleForceGenerator fg)
	{
		registrations.Add(new ParticleForceRegistration { particle = particle, fg = fg });
	}

	public void Remove(Particle particle, ParticleForceGenerator fg)
	{
		registrations.Remove(new ParticleForceRegistration { particle = particle, fg = fg });
	}

	public void Clear() => registrations.Clear();

	/*
	private void FixedUpdate()
	{
		UpdateForces(Time.fixedDeltaTime);
	}
	*/

	public void UpdateForces(float duration)
	{
		foreach (ParticleForceRegistration reg in registrations)
		{
			reg.fg.UpdateForce(reg.particle, duration);
		}
	}
}
