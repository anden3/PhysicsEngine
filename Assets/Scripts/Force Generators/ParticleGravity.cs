using UnityEngine;

[RequireComponent(typeof(Particle))]
public class ParticleGravity : MonoBehaviour, ParticleForceGenerator
{
	public Vector3 gravity;

	private Particle particle;

	private void Awake()
	{
		particle = GetComponent<Particle>();
	}

	private void Start()
	{
		ParticleForceRegistry.registry.Add(particle, this);
	}

	public void UpdateForce(Particle particle, float duration)
	{
		if (!particle.HasFiniteMass()) return;

		particle.AddForce(gravity * particle.mass);
	}
}
