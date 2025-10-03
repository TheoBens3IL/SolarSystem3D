using UnityEngine;

/// <summary>
/// ControlCamera: Handles mouse-based navigation similar to Unity Scene View.
/// - Right mouse button: rotate camera.
/// - Mouse wheel: zoom in/out.
/// - z,q,s,d keys: move camera forward, left, backward, right.
/// - r,f keys: move camera up/down.
/// Attach this script to your Camera GameObject.
/// </summary>
public class ControllCamera : MonoBehaviour
{
    public float moveSpeed = 2000f;       // Translation speed
    public float mouseSensitivity = 2f;   // Mouse rotation sensitivity
    public float scrollSpeed = 100f;      // Zoom speed

    private float rotationX;
    private float rotationY;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y; // rotation autour de l'axe Y
        rotationY = angles.x; // rotation autour de l'axe X
    }

    void Update()
    {
        // Mouse rotation (right-click held)
        if (Input.GetMouseButton(1))
        {
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f);

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }

        // Keyboard movement (WASD)
        Vector3 direction = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );

        Vector3 displacement = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;
        transform.position += displacement;

        // Move up/down with R and F
        if (Input.GetKey(KeyCode.R))
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.F))
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        // Zoom with mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * scrollSpeed;
    }
}
