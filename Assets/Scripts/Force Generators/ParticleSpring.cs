/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[AddComponentMenu("Particle Physics/Connectors/Spring")]
public class ParticleSpring : ParticleForceGenerator
{
    [Header("Connection Settings")]
    public Particle other;

    [Header("Spring Settings")]
    public float springConstant;
    public float restLength;

    private void OnValidate()
    {
        if (other != null && restLength == 0)
        {
            restLength = Vector3.Distance(transform.localPosition, other.position);
        }
    }

    public override void UpdateForce(float deltaTime)
    {
        Vector3 springVec = particle.position - other.position;
        float force = (springVec.magnitude - restLength) * springConstant;

        particle.AddForce(springVec.normalized * -force);
    }
}
