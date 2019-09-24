/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[RequireComponent(typeof(Sphere))]
public class Draggable : MonoBehaviour
{
    public float dragCoefficient;

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
            dragging = false;
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = particle.position.z;

        dragDist = mouseWorldPos - particle.position;

        if (!dragging && dragDist.sqrMagnitude <= (particle.radius * particle.radius))
        {
            dragging = true;
        }
    }

    private void FixedUpdate()
    {
        if (dragging)
        {
            Debug.Log(dragDist * dragCoefficient);
            particle.AddForce(dragDist * dragCoefficient);
        }
    }
}
