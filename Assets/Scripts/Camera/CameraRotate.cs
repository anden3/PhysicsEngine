using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraRotate : MonoBehaviour
{
    public Transform target;

    [Header("Speeds")]
    public float rotateSpeed = 1.0f;
    public float zoomSpeed = 1.0f;
    public float followSpeed = 0.1f;

    [Header("Key Bindings")]
    public InputAction lookXAction;
    public InputAction lookYAction;
    public InputAction zoomAction;

    private float distance;
    private Vector3 followVelocity = Vector3.zero;

    private void Start()
    {
        distance = Vector3.Distance(transform.position, target.position);
        transform.LookAt(target, Vector3.up);

        // zoomAction.performed += OnZoom;
    }

    private void OnEnable()
    {
        lookXAction.Enable();
        lookYAction.Enable();
        zoomAction.Enable();
    }

    private void OnDisable()
    {
        lookXAction.Disable();
        lookYAction.Disable();
        zoomAction.Disable();
    }

    private void LateUpdate()
    {
        transform.RotateAround(target.position, Vector3.up, lookXAction.ReadValue<float>() * rotateSpeed);
        transform.RotateAround(target.position, transform.right, lookYAction.ReadValue<float>() * -rotateSpeed);

        transform.LookAt(target, Vector3.up);

        distance = Mathf.Max(target.localScale.x, distance - zoomAction.ReadValue<float>() * zoomSpeed);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target.position + distance * -transform.forward,
            ref followVelocity, followSpeed
        );
    }
}
