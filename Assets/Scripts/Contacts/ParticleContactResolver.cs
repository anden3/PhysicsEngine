/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

public class ParticleContactResolver : MonoBehaviour
{
	public int iterations;

	public void ResolveContacts(List<ParticleContact> contacts, float duration)
	{
        int iterationsUsed = 0;

		while (iterationsUsed < iterations)
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
			contacts[maxIndex].Resolve(duration);
			iterationsUsed++;
		}
	}
}
