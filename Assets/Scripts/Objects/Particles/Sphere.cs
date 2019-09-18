/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using System.Collections.Generic;

public class Sphere : Particle
{
	[Header("Specific Properties")]
	public float radius;

	private void OnValidate()
	{
		transform.localScale = new Vector3(radius, radius, radius) * 2;
	}

	public override void GetContacts(ref List<ParticleContact> contacts)
	{
		float bottom = transform.localPosition.y - radius;

		if (bottom > 0)
			return;

		ParticleContact contact = new ParticleContact();

		contact.first = this;
		contact.penetration = -bottom;
		contact.contactNormal = Vector3.up;
		contact.restitution = bounciness;

		contacts.Add(contact);
	}
}
