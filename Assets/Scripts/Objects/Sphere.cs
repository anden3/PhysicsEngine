/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[AddComponentMenu("Particle Physics/Objects/Sphere")]
public class Sphere : Particle
{
	[Header("Specific Properties")]
	public float radius = 0.5f;

	private void OnValidate()
	{
		transform.localScale = new Vector3(radius, radius, radius) * 2;
	}
}
