/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

[AddComponentMenu("Particle Physics/Connectors/Cable")]
public class ParticleCable : ParticleLink
{
    [Header("Cable Settings")]
    public float maxLength;
    public float restitution;

    private void OnValidate()
    {
        if (first != null && second != null && maxLength == 0)
        {
            maxLength = Vector3.Distance(first.position, second.position);
        }
    }

    public override void GetContacts(ref List<ParticleContact> contacts)
    {
        float length = currentLength;

        // Only apply force if overextended.
        if (length < maxLength) return;

        contacts.Add(new ParticleContact
        {
            first = first,
            second = second,
            restitution = restitution,
            penetration = length - maxLength,
            contactNormal = (second.position - first.position).normalized
        });
    }
}
