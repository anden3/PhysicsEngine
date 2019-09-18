using UnityEngine;

using System.Collections.Generic;

public class World : MonoBehaviour
{
	public List<RigidBody> bodies;

	private void FixedUpdate()
	{
		foreach (RigidBody body in bodies)
		{
			// registry.UpdateForces(Time.fixedDeltaTime);

			body.Integrate(Time.fixedDeltaTime);
		}
	}
}
