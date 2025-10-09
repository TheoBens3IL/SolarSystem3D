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
    public float moveSpeed = 200f;        // Translation speed
    public float mouseSensitivity = 2f;   // Mouse rotation sensitivity (réutilisé en follow mode)
    public float scrollSpeed = 100f;      // Zoom speed (réutilisé en follow mode)

    private float rotationX;
    private float rotationY;

    // FOLLOW MODE STATE
    private bool followMode = false;
    private Transform followTarget;
    private Vector3 followOffset = Vector3.zero;
    private float followSmoothSpeed = 10f;
    private float followMinDistance = 0.5f;
    private float followMaxDistance = 3000f;
    // note: on n'utilise plus de paramètres distincts pour rotate/zoom — on réutilise mouseSensitivity/scrollSpeed

    private float followMinVerticalAngle = 5f;
    private float followMaxVerticalAngle = 175f;

    // saved camera state for restoration
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private Transform savedParent;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y; // rotation autour de l'axe Y
        rotationY = angles.x; // rotation autour de l'axe X
    }

    void LateUpdate()
    {
        if (!followMode)
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
        else
        {
            // FOLLOW MODE: support zoom (wheel) and rotation (right-drag) around followTarget
            if (followTarget == null)
            {
                // fallback stop follow
                StopFollow();
                return;
            }

            // Zoom using mouse wheel (adjust offset distance) — utilise scrollSpeed (même que hors follow)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 1e-6f)
            {
                float dist = followOffset.magnitude;
                dist = Mathf.Clamp(dist - scroll * scrollSpeed, followMinDistance, followMaxDistance);
                followOffset = followOffset.normalized * dist;
            }

            // Rotation: right mouse drag — utilise mouseSensitivity (même que hors follow)
            if (Input.GetMouseButton(1))
            {
                float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
                float pitch = -Input.GetAxis("Mouse Y") * mouseSensitivity;

                // apply yaw around world up
                followOffset = Quaternion.AngleAxis(yaw, Vector3.up) * followOffset;

                // apply pitch around camera right (relative to current offset)
                Vector3 right = Vector3.Cross(Vector3.up, followOffset).normalized;
                followOffset = Quaternion.AngleAxis(pitch, right) * followOffset;

                // clamp vertical angle
                float angleFromUp = Vector3.Angle(followOffset, Vector3.up);
                float clamped = Mathf.Clamp(angleFromUp, followMinVerticalAngle, followMaxVerticalAngle);
                if (!Mathf.Approximately(angleFromUp, clamped))
                {
                    Vector3 horiz = Vector3.ProjectOnPlane(followOffset, Vector3.up).normalized;
                    float dist = followOffset.magnitude;
                    // rebuild offset with clamped angle preserving horizontal direction
                    followOffset = Quaternion.AngleAxis(90f - clamped, right) * (horiz * dist);
                }
            }

            // Exit follow on movement keys (W/A/S/D or Z/Q/S/D)
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Q) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A))
            {
                StopFollow();
                return;
            }

            // Smoothly move camera to target position (target + offset)
            Vector3 desiredPos = followTarget.position + followOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followSmoothSpeed * Time.deltaTime));

            // Look at target
            transform.LookAt(followTarget.position);
        }
    }

    /// <summary>
    /// Start following a target. Camera state is saved and restored on StopFollow().
    /// followOffset should be a world-space offset (target.position + offset = desired camera pos).
    /// </summary>
    public void StartFollow(Transform target, Vector3 worldOffset,
        float smoothSpeed = 10f, float minDistance = 0.5f, float maxDistance = 2000f,
        float rotateSpeed = 0.2f, float zoomSpeed = 50f, float minVertAngle = 5f, float maxVertAngle = 175f)
    {
        if (target == null) return;

        // save state
        savedPosition = transform.position;
        savedRotation = transform.rotation;
        savedParent = transform.parent;

        followTarget = target;
        followOffset = worldOffset;
        followSmoothSpeed = smoothSpeed;
        followMinDistance = minDistance;
        followMaxDistance = maxDistance;
        // on réutilise mouseSensitivity/scrollSpeed au lieu de paramètres distincts
        followMinVerticalAngle = minVertAngle;
        followMaxVerticalAngle = maxVertAngle;

        followMode = true;
    }

    public void StopFollow()
    {
        if (!followMode) return;

        followMode = false;
        followTarget = null;

        // restore saved camera transform
        transform.parent = savedParent;
        transform.position = savedPosition;
        transform.rotation = savedRotation;
    }

    public bool IsFollowing() => followMode;
}
