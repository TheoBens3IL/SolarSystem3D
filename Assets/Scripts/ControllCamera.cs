using UnityEngine;

/// <summary>
/// ControlCamera: Handles mouse-based navigation similar to Unity Scene View.
/// - Right mouse button: rotate camera.
/// - Mouse wheel: zoom in/out.
/// - Middle mouse button (mouse wheel click): pan camera.
/// Attach this script to your Camera GameObject.
/// </summary>
public class ControllCamera : MonoBehaviour
{
    #region Serialized Fields
    [Header("Rotation Settings")]
    [Tooltip("Mouse sensitivity for horizontal and vertical rotation.")]
    [SerializeField] private float rotationSensitivity = 100f;

    [Tooltip("Minimum vertical angle (looking down).")]
    [SerializeField] private float minPitch = -80f;

    [Tooltip("Maximum vertical angle (looking up).")]
    [SerializeField] private float maxPitch = 80f;

    [Header("Zoom Settings")]
    [Tooltip("Speed of zoom when using mouse wheel.")]
    [SerializeField] private float zoomSpeed = 10f;

    [Tooltip("Minimum distance (zoom in limit).")]
    [SerializeField] private float minZoomDistance = 2f;

    [Tooltip("Maximum distance (zoom out limit).")]
    [SerializeField] private float maxZoomDistance = 200f;

    [Header("Pan Settings")]
    [Tooltip("Speed of panning when holding middle mouse button.")]
    [SerializeField] private float panSpeed = 10f;

    [Header("Cursor Settings")]
    [Tooltip("Hide and lock the cursor while rotating.")]
    [SerializeField] private bool lockCursorWhenRotating = true;

    

    #endregion

    #region Private Variables
    // Current rotation angles
    private float pitch = 0f; // Up/Down rotation
    private float yaw = 0f;   // Left/Right rotation

    // Current zoom distance
    private float currentZoomDistance = 10f;

    // Original camera offset direction
    private Vector3 cameraOffset = Vector3.back; // behind by default
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        // Initialize rotation and offset
        Vector3 angles = transform.rotation.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;

        cameraOffset = transform.forward * -currentZoomDistance;
    }

    private void Update()
    {
        HandleRotation();
        HandleZoom();
        HandlePan();
    }
    #endregion

    #region Input Methods
    /// <summary>
    /// Handles rotation with right mouse button.
    /// </summary>
    private void HandleRotation()
    {
        // Right mouse button held down
        if (Input.GetMouseButton(1))
        {
            if (lockCursorWhenRotating)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            float mouseX = Input.GetAxis("Mouse X") * rotationSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSensitivity * Time.deltaTime;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // Apply rotation to camera
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        else
        {
            // Release cursor when not rotating
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Handles zooming with the mouse wheel.
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoomDistance -= scroll * zoomSpeed;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);

            // Move camera forward/backwards along its forward axis
            transform.position += transform.forward * scroll * zoomSpeed;
        }
    }

    /// <summary>
    /// Handles panning with the middle mouse button.
    /// </summary>
    private void HandlePan()
    {
        if (Input.GetMouseButton(2))
        {
            float mouseX = -Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            float mouseY = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;

            // Move camera laterally and vertically relative to its orientation
            transform.Translate(new Vector3(mouseX, mouseY, 0f), Space.Self);
        }
    }

    /// <summary>
    /// Rotates the planet around its own axis based on real-world rotation period
    /// and a speed multiplier for visibility.
    /// Call this method from Update().
    /// </summary>
   

    #endregion
}
