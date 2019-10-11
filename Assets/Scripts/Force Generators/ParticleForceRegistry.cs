/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

// Holds all the force generators and the particles that they apply to.
[AddComponentMenu("Particle Physics/Force Generators/Force Generator Registry")]
public class ParticleForceRegistry : MonoBehaviour
{
    private static List<ParticleForceGenerator> generators
        = new List<ParticleForceGenerator>();

    public static void Register(ParticleForceGenerator gen)
        => generators.Add(gen);

    public static void Unregister(ParticleForceGenerator gen)
        => generators.Remove(gen);

    public void UpdateForces(float deltaTime)
	{
		foreach (var gen in generators)
		    gen.UpdateForce(deltaTime);
	}
}
