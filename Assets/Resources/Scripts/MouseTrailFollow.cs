using UnityEngine;

public class MouseTrailFollow : MonoBehaviour
{
    [Tooltip("Camera used for the start screen (usually Main Camera).")]
    public Camera cam;

    [Tooltip("Distance from the camera where the trail should appear.")]
    public float distanceFromCamera = 5f;

    [Tooltip("Controls how snappy the trail follows the mouse.")]
    public float followSpeed = 30f;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        // Mouse position in screen space
        Vector3 mousePos = Input.mousePosition;

        // Use a fixed distance in front of the camera
        mousePos.z = distanceFromCamera;

        // Convert to world position
        Vector3 targetWorldPos = cam.ScreenToWorldPoint(mousePos);

        // Smooth follow for a nice floaty feeling
        transform.position = Vector3.Lerp(
            transform.position,
            targetWorldPos,
            Time.deltaTime * followSpeed
        );
    }
}
