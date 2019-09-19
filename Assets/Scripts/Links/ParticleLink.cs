/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 *
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

public class ParticleLink : MonoBehaviour, IParticleContactGenerator
{
    [Header("Endpoints")]
    public Particle first;
    public Particle second;

    protected float currentLength
        => Vector3.Distance(first.position, second.position);

    private void Start() => ParticleContactResolver.Register(this);
    private void OnDestroy() => ParticleContactResolver.Unregister(this);

    public virtual void GetContacts(ref List<ParticleContact> contacts) { }
}
