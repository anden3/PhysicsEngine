/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class Particle : MonoBehaviour, IParticleContactGenerator
{
	[Header("Physical Properties")]
	public float mass;

	[Range(0.0f, 1.0f)]
	public float bounciness;
	[Range(0.0f, 1.0f)]
	public float damping;
    [Range(0.0f, 1.0f)]
    public float groundDamping;

	public float inverseMass { get; protected set; }
    public Vector3 position
    {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }

	[Header("Starting Conditions")]
	public Vector3 velocity;
	public Vector3 acceleration { get; set; }

    [Header("Object References")]
    public Text text;

	protected Vector3 forceAccum;

    protected bool onGround;

	public bool HasFiniteMass() => inverseMass > 0.0f;

	protected virtual void Awake()
	{
		inverseMass = 1.0f / mass;
    }

    private void Start() => ParticleContactResolver.Register(this);
    private void OnDestroy() => ParticleContactResolver.Unregister(this);

    public void AddForce(in Vector3 force) => forceAccum += force;
    protected void ClearAccumulator() => forceAccum = Vector3.zero;

    public void Integrate(float duration)
	{
		Debug.Assert(duration > 0.0f);

		// Only accept finite masses.
		if (!HasFiniteMass()) return;

		// Update position.
		transform.position += velocity * duration;

		// Get acceleration from forces.
		Vector3 resultingAcc = acceleration;
		resultingAcc += forceAccum * inverseMass;

		// Update velocity.
		velocity += resultingAcc * duration;

		// Add drag.
		velocity *= Mathf.Pow(damping, duration);

        if (onGround)
        {
            velocity *= Mathf.Pow(groundDamping, duration);
        }

		// Clear forces.
		ClearAccumulator();

        //Update velocity text.
        Vector3 textPos = transform.position;
        textPos.y += 2;
        text.transform.position = Camera.main.WorldToScreenPoint(textPos);

        text.text = $"Velocity: {((velocity.magnitude < 0.8f) ? Vector3.zero : velocity)}";
    }

    public virtual void GetContacts(ref List<ParticleContact> contacts) { }
}
