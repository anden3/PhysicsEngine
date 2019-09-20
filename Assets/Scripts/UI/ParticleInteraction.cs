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
    void MouseUp();
}

public class ParticleInteraction : MonoBehaviour, IMouseMessageTarget
{
	public ParticlePhysicsEngine simulation;
	public Particle particle;

    private bool mouseDown;
    private bool handleInput = true;

	private Image arrow;
	private Text label;

	private Vector3 start;
	private Vector3 end;

	private void Awake()
	{
		arrow = GetComponent<Image>();
		label = GetComponentInChildren<Text>();
	}

	public void MouseMove()
	{
        if (!handleInput) return;

        if (mouseDown)
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
        else
        {
            // Particle follows mouse.
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 60;
            
            particle.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        }
	}

	public void MouseDown()
	{
        if (handleInput)
        {
            // Set arrow start position.
            start = Camera.main.WorldToScreenPoint(particle.transform.position);
            arrow.transform.position = start;
            mouseDown = true;
        }
    }

    public void MouseUp()
    {
        if (handleInput)
        {
            simulation.StartSimulation();

            arrow.gameObject.SetActive(false);
            mouseDown = false;
        }
    }
    public void StraightDown()
    {
        handleInput = false;
        arrow.gameObject.SetActive(false);

        particle.transform.position = new Vector3(0, Camera.main.transform.position.y, 60);
        particle.velocity = Vector3.zero;

        simulation.StartSimulation();
    }

    public void Sideways()
    {
        handleInput = false;
        arrow.gameObject.SetActive(false);

        particle.transform.position = new Vector3(-10, Camera.main.transform.position.y, 60);
        particle.velocity = new Vector3(5, 0, 0);

        simulation.StartSimulation();
    }
}
