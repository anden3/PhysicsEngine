/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[AddComponentMenu("Particle Physics/Connectors/Standalone Spring")]
public class ParticleStandaloneSpring : ParticleForceGenerator
{
    [Header("Connection Settings")]
    public Particle first;
    public Particle second;

    [Header("Spring Settings")]
    public float springConstant = 2;
    public float restLength;

    private void OnValidate()
    {
        if (first != null && second != null && restLength == 0)
        {
            restLength = Vector3.Distance(first.position, second.position);
        }
    }

    protected override void Awake() {  }

    public override void UpdateForce(float deltaTime)
    {
        // Get the force from the spring according to Hooke's law.
        Vector3 springVec = first.position - second.position;
        float force = (springVec.magnitude - restLength) * springConstant;

        // Add forces to the particles.
        first.AddForce(springVec.normalized * -force);
        second.AddForce(springVec.normalized * force);

        // Move and rotate spring object between the particles.
        transform.localPosition = Vector3.Lerp(first.position, second.position, 0.5f);
        transform.localRotation = Quaternion.FromToRotation(Vector3.up, springVec);

        // Make spring object properly scaled.
        Vector3 newScale = transform.localScale;
        newScale.y = springVec.magnitude * 0.5f;
        transform.localScale = newScale;
    }
}
