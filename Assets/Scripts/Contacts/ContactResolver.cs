/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 */

using UnityEngine;

using System.Collections.Generic;

public class ContactResolver : MonoBehaviour
{
    public int maxPositionIterations = 4;
    public int maxVelocityIterations = 4;

    public float penetrationEpsilon;

    public void ResolveContacts(List<Contact> contacts, float deltaTime)
    {
        if (contacts.Count == 0)
            return;

        PrepareContacts(contacts, deltaTime);
        AdjustPositions(contacts, deltaTime);
        AdjustVelocities(contacts, deltaTime);
    }

    private void PrepareContacts(List<Contact> contacts, float deltaTime)
    {
        foreach (Contact contact in contacts)
        {
            contact.CalculateInternals(deltaTime);
        }
    }

    private void AdjustPositions(List<Contact> contacts, float deltaTime)
    {
        int iterationsUsed = 0;

        while (iterationsUsed < maxPositionIterations)
        {
            // Find biggest penetration.
            float max = penetrationEpsilon;
            Contact match = null;

            foreach (Contact contact in contacts)
            {
                if (contact.penetration > max)
                {
                    max = contact.penetration;
                    match = contact;
                }
            }

            if (match == null)
                break;

            match.MatchAwakeState();
            match.ApplyPositionChange(
                max,
                out Vector3[] linearChange,
                out Vector3[] angularChange
            );

            foreach (Contact contact in contacts)
            {
                for (int bodyIndex = 0; bodyIndex < 2; bodyIndex++)
                {
                    if (match.bodies[bodyIndex] == null)
                        continue;

                    for (int b = 0; b < 2; b++)
                    {
                        if (contact.bodies[bodyIndex] == match.bodies[b])
                        {
                            Vector3 deltaPosition =
                                linearChange[b] + Vector3.Cross(angularChange[b], contact.relativePositions[bodyIndex]);

                            contact.penetration += Vector3.Dot(deltaPosition, contact.normal) * ((bodyIndex == 1) ? 1 : -1);
                        }
                    }
                }
            }

            iterationsUsed++;
        }
    }

    private void AdjustVelocities(List<Contact> contacts, float deltaTime)
    {
        int iterationsUsed = 0;

        while (iterationsUsed < maxVelocityIterations)
        {
            // Find largest closing velocity.
            float max = float.Epsilon;
            Contact match = null;

            foreach (Contact contact in contacts)
            {
                if (contact.desiredDeltaVelocity > max)
                {
                    max = contact.desiredDeltaVelocity;
                    match = contact;
                }
            }

            if (match == null)
                break;

            match.MatchAwakeState();
            match.ApplyVelocityChange(
                out Vector3[] velocityChange,
                out Vector3[] rotationChange
            );

            foreach (Contact contact in contacts)
            {
                for (int bodyIndex = 0; bodyIndex < 2; bodyIndex++)
                {
                    if (match.bodies[bodyIndex] == null)
                        continue;

                    for (int b = 0; b < 2; b++)
                    {
                        if (match.bodies[bodyIndex] == contact.bodies[b])
                        {
                            Vector3 deltaVelocity =
                                velocityChange[b] + Vector3.Cross(rotationChange[b], contact.relativePositions[bodyIndex]);

                            contact.contactVelocity +=
                                contact.worldToContact.Transform(deltaVelocity) * ((bodyIndex == 1) ? -1 : 1);

                            contact.CalculateDesiredDeltaVelocity(deltaTime);
                        }
                    }
                }
            }

            iterationsUsed++;
        }
    }
}
