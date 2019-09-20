/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

[AddComponentMenu("Particle Physics/Objects/Sphere")]
public class Sphere : Particle
{
	[Header("Specific Properties")]
	public float radius = 0.5f;

	private void OnValidate()
	{
		transform.localScale = new Vector3(radius, radius, radius) * 2;
	}

	public override void GetContacts(ref List<ParticleContact> contacts)
	{
		float bottom = position.y - radius;
        onGround = bottom <= 0;

		if (!onGround)
			return;

        contacts.Add(new ParticleContact
        {
            first = this,
            penetration = -bottom,
            contactNormal = Vector3.up,
            restitution = bounciness
        });
	}
}
