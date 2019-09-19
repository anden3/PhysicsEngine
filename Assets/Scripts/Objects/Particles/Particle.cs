/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class Particle : MonoBehaviour
{
	[Header("Physical Properties")]
	public float mass;

	[Range(0.0f, 1.0f)]
	public float bounciness = 0.5f;
	[Range(0.0f, 1.0f)]
	public float airResistanceCoef = 0.5f;
    [Range(0.0f, 1.0f)]
    public float groundFrictionCoef = 0.5f;

	public float inverseMass { get; protected set; }

	// [Header("Starting Conditions")]
    public Vector3 velocity
    {
        get => GetVelocity();
        set => SetVelocity(value);
    }
	public Vector3 acceleration { get; set; }

    [Header("Object References")]
    public Text text;

	protected Vector3 forceAccum;

    protected bool onGround;
    protected Vector3 startPosition;

    protected Vector3 lastPos;

	public bool HasFiniteMass() => inverseMass > 0.0f;

	protected virtual void Awake()
	{
		inverseMass = 1.0f / mass;
        startPosition = transform.localPosition;
        lastPos = startPosition;
    }

    public virtual void Reset()
    {
        transform.localPosition = startPosition;
        velocity = Vector3.zero;
        onGround = false;
        text.text = "";
    }

    public void AddForce(in Vector3 force) => forceAccum += force;
    protected void ClearAccumulator() => forceAccum = Vector3.zero;

    public void SetMass(float newMass)
    {
        mass = newMass;
        inverseMass = 1.0f / mass;
    }

    public Vector3 GetVelocity()
        => (transform.localPosition - lastPos) * Time.fixedDeltaTime;

    public void SetVelocity(Vector3 v)
    {

    }

    public void Integrate(float deltaTime)
	{
        // Only accept finite masses.
        if (!HasFiniteMass()) return;

        /*
        // Update position.
		transform.position += velocity * deltaTime; 
        */

        // Get acceleration from forces.
        Vector3 resultingAcc = acceleration;
        resultingAcc += forceAccum * inverseMass;

        Vector3 pos = transform.localPosition;

        transform.localPosition += pos - lastPos
            + resultingAcc * deltaTime * deltaTime;

        lastPos = pos;

        /*
		// Update velocity.
		velocity += resultingAcc * deltaTime;

		// Simulate air resistance.
		velocity *= Mathf.Pow(airResistanceCoef, deltaTime);

        if (onGround)
        {
            // Simulate friction.
            velocity *= Mathf.Pow(groundFrictionCoef, deltaTime);
        }
        */

        // Clear forces.
        ClearAccumulator();

        //Update velocity text.
        Vector3 textPos = transform.position;
        textPos.y += 2;
        text.transform.position = Camera.main.WorldToScreenPoint(textPos);

        text.text = $"Velocity: {velocity} m/s.";
    }

    public virtual void GetContacts(ref List<ParticleContact> contacts) { }
}
