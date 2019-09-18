using UnityEngine;

using System.Collections.Generic;

public class ParticleContactResolver : MonoBehaviour
{
	public int iterations;
	protected int iterationsUsed;

	public void ResolveContacts(List<ParticleContact> contacts, float duration)
	{
		iterationsUsed = 0;

		while (iterationsUsed < iterations)
		{
			// Find contact with largest closing velocity.
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
