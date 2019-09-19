/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

public class ParticleAnchoredSpring : ParticleForceGenerator
{
    [Header("Connection Settings")]
    public Vector3 anchor;

    [Header("Spring Settings")]
    public float springConstant;
    public float restLength;

    public override void UpdateForce(float deltaTime)
    {
        Vector3 springVec = particle.position - anchor;
        float length = Mathf.Abs(springVec.magnitude - restLength) * springConstant;

        particle.AddForce(springVec.normalized * -length);
    }
}
