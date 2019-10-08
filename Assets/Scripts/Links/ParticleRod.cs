﻿/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

[AddComponentMenu("Particle Physics/Connectors/Rod")]
public class ParticleRod : ParticleLink, IParticleContactGenerator
{
    [Header("Rod Settings")]
    public float length;

    private void OnValidate()
    {
        if (first != null && second != null && length == 0)
        {
            length = Vector3.Distance(first.position, second.position);
        }
    }

    public override void GetContacts(ref List<ParticleContact> contacts)
    {
        float currentLength = base.currentLength;

        // Only apply force if stretched in some way.
        if (currentLength == length) return;

        ParticleContact contact = new ParticleContact
        {
            first = first,
            second = second,
            restitution = 0
        };
        
        if (currentLength > length)
        {
            contact.contactNormal = (second.position - first.position).normalized;
            contact.penetration = currentLength - length;
        }
        else
        {
            contact.contactNormal = -1 * (second.position - first.position).normalized;
            contact.penetration = length - currentLength;
        }

        contacts.Add(contact);
    }
}