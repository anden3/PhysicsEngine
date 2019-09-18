/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

public interface IMouseMessageTarget : IEventSystemHandler
{
	void MouseMove();
	void MouseDown();
}

public class VelocityArrow : MonoBehaviour, IMouseMessageTarget
{
	public ParticleWorld simulation;
	public Particle particle;

	private Image arrow;
	private Text label;

	private Vector3 start;
	private Vector3 end;

	private void Awake()
	{
		arrow = GetComponent<Image>();
		label = GetComponentInChildren<Text>();
	}

	private void Start()
	{
		start = Camera.main.WorldToScreenPoint(particle.transform.position);
		arrow.transform.position = start;
	}

	public void MouseMove()
	{
		end = Input.mousePosition;

		// Set arrow to be between particle and mouse.
		arrow.transform.position = Vector3.Lerp(start, end, 0.5f);

		// Change size of arrow.
		arrow.rectTransform.sizeDelta = new Vector2(
			Vector3.Distance(start, end),
			arrow.rectTransform.sizeDelta.y
		);

		Vector3 direction = start - end;

		if (direction != Vector3.zero)
		{
			// Rotate arrow.
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			// Flip text if on the right side of the particle.
			Vector3 labelScale = label.transform.localScale;
			labelScale.x = (angle > 90 || angle < -90) ? -1 : 1;
			labelScale.y = (angle > 90 || angle < -90) ? -1 : 1;
			label.transform.localScale = labelScale;
		}

		// Get velocity in world units instead of pixels.
		Vector3 velocity = Camera.main.ScreenToWorldPoint(end) - particle.transform.position;
		velocity.z = 0;
		particle.velocity = velocity;

		label.text = velocity.ToString();
	}

	public void MouseDown()
	{
		simulation.StartSimulation();

		arrow.gameObject.SetActive(false);
		gameObject.SetActive(false);
	}
}
