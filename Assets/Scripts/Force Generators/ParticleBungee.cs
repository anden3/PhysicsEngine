/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[AddComponentMenu("Particle Physics/Connectors/Bungee")]
public class ParticleBungee : ParticleForceGenerator
{
    [Header("Connection Settings")]
    public Particle other;

    [Header("Spring Settings")]
    public float springConstant;
    public float restLength;

    public override void UpdateForce(float deltaTime)
    {
        Vector3 springVec = particle.position - other.position;

        // Only apply force when bungee cord is being extended.
        float length = springVec.magnitude;
        if (length <= restLength) return;

        length = (restLength - length) * springConstant;

        particle.AddForce(springVec.normalized * -length);
    }
}
