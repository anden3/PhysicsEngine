/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[RequireComponent(typeof(Sphere))]
public class Draggable : MonoBehaviour
{
    public float dragCoefficient;
    public Texture2D dragCursor;

    private Sphere particle;

    private bool dragging = false;
    private Vector3 dragDist;

    private void Awake()
    {
        particle = GetComponent<Sphere>();
    }

    private void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            Cursor.SetCursor(null, Vector3.zero, CursorMode.Auto);
            dragging = false;
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = particle.position.z;

        dragDist = mouseWorldPos - particle.position;

        if (!dragging && dragDist.sqrMagnitude <= (particle.radius * particle.radius))
        {
            dragging = true;

            Cursor.SetCursor(
                dragCursor,
                new Vector2(dragCursor.width / 2, dragCursor.height / 2),
                CursorMode.Auto
            );
        }
    }

    private void FixedUpdate()
    {
        if (dragging)
        {
            particle.AddForce(dragDist * dragCoefficient);
        }
    }
}
