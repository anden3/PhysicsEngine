/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[RequireComponent(typeof(Particle))]
public class ParticleForceGenerator : MonoBehaviour
{
    protected Particle particle;

    private void Awake()
    {
        particle = GetComponent<Particle>();
    }

    private void Start() => ParticleForceRegistry.Register(this);
    private void OnDestroy() => ParticleForceRegistry.Unregister(this);

    public virtual void UpdateForce(float deltaTime) { }
}