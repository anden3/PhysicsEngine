using UnityEngine;

using System.Collections.Generic;

[AddComponentMenu("Rigid Body Physics/Rigid Body Physics Engine")]
public class RigidBodyPhysicsEngine : MonoBehaviour
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
