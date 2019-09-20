/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

public interface IParticleContactGenerator
{
    void GetContacts(ref List<ParticleContact> contacts);
}

[AddComponentMenu("Particle Physics/Contacts/Contact Resolver")]
public class ParticleContactResolver : MonoBehaviour
{
    private static List<IParticleContactGenerator> generators
        = new List<IParticleContactGenerator>();

    public static void Register(IParticleContactGenerator gen)
        => generators.Add(gen);

    public static void Unregister(IParticleContactGenerator gen)
        => generators.Remove(gen);

    public int iterations;
    public bool autoIterations = true;

    private List<ParticleContact> contacts = new List<ParticleContact>();

    private void GenerateContacts()
    {
        contacts.Clear();

        // Accumulate contacts.
        foreach (var gen in generators)
        {
            gen.GetContacts(ref contacts);
        }
    }

    public void ResolveContacts(float deltaTime)
	{
        GenerateContacts();

        int iterationsUsed = 0;
        int iterMax = autoIterations ? contacts.Count * 2 : iterations;

		while (iterationsUsed < iterMax)
		{
			// Find contact with largest closing velocity (largest negative value).
			float max = float.MaxValue;
			int maxIndex = contacts.Count;

			for (int i = 0; i < contacts.Count; i++)
			{
				float sepVel = contacts[i].CalculateSeparatingVelocity();

				if (sepVel < max && (sepVel < 0 || contacts[i].penetration > 0))
				{
					max = sepVel;
					maxIndex = i;
				}
			}

			// Check if we have anything to resolve.
			if (maxIndex == contacts.Count) break;

			// Resolve the contact.
			contacts[maxIndex].Resolve(deltaTime);
			iterationsUsed++;
		}
	}
}
