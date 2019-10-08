/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraRotate : MonoBehaviour
{
    public Transform target;
    [Space]
    public float followSpeed = 0.1f;
    public float rotateSpeed = 1.0f;
    public float zoomSpeed = 1.0f;

    private float distance;
    private Vector3 followVelocity = Vector3.zero;

    private void OnEnable() => BodySettings.targetChanged += SwitchTarget;
    private void OnDisable() => BodySettings.targetChanged -= SwitchTarget;

    private void Start()
    {
        distance = Vector3.Distance(transform.position, target.position);
        transform.LookAt(target, Vector3.up);
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            transform.RotateAround(target.position, Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed);
            transform.RotateAround(target.position, transform.right, Input.GetAxis("Mouse Y") * -rotateSpeed);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            distance = Mathf.Max(target.localScale.x, distance - Input.mouseScrollDelta.y * zoomSpeed);
        }

        transform.LookAt(target, Vector3.up);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target.position + distance * -transform.forward,
            ref followVelocity, followSpeed
        );
    }

    private void SwitchTarget(Transform newTarget)
    {
        target = newTarget;

        transform.LookAt(target, Vector3.up);
        distance = Vector3.Distance(transform.position, target.position);
    }
}
