/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

[RequireComponent(
    typeof(ParticleForceRegistry)
)]
[AddComponentMenu("Particle Physics/Particle Physics Engine")]
public class ParticlePhysicsEngine : MonoBehaviour
{
    public enum SimulationState
    {
        Playing,
        Paused,
        SteppingForward,
        SteppingBackward
    }

    [Header("Global Settings")]
    public SimulationState state = SimulationState.Playing;

    private SimulationState initialState;

    private ParticleForceRegistry registry;

    private static List<Particle> particles = new List<Particle>();

    public static void Register(Particle p) => particles.Add(p);
    public static void Unregister(Particle p) => particles.Remove(p);

    public void StartSimulation() => state = SimulationState.Playing;
    public void PauseSimulation() => state = SimulationState.Paused;
    public void StepForward() => state = SimulationState.SteppingForward;
    public void StepBackward() => state = SimulationState.SteppingBackward;

	private void Awake()
	{
        initialState = state;
        registry = GetComponent<ParticleForceRegistry>();
	}

    private void FixedUpdate()
	{
		if (state == SimulationState.Paused)
            return;

        float timeStep = Time.fixedDeltaTime * TimeScale.CurrentTimeScale;

        if (state == SimulationState.SteppingBackward)
            timeStep *= -1;

        // Update all force generators.
        registry.UpdateForces(timeStep);

		IntegrateParticles(timeStep);

        if (state == SimulationState.SteppingForward ||
            state == SimulationState.SteppingBackward)
        {
            state = SimulationState.Paused;
        }
    }

	private void IntegrateParticles(float deltaTime)
	{
        foreach (Particle p in particles)
		    p.Integrate(deltaTime);
	}

    public void ResetSimulation()
    {
        foreach (Particle p in particles)
            p.ParticleReset();

        state = initialState;
    }
}
