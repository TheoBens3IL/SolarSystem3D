using UnityEngine;

/// <summary>
/// Contrôle de la caméra libre dans la scène.
/// - Clic droit : rotation.
/// - Molette : zoom.
/// - Z/Q/S/D : déplacement horizontal.
/// - R/F : montée/descente.
/// Peut aussi suivre une cible (mode "Follow").
/// </summary>
public class ControllCamera : MonoBehaviour
{
    [Header("Contrôle libre")]
    public float moveSpeed = 200f;
    public float mouseSensitivity = 2f;
    public float scrollSpeed = 100f;

    private float rotationX;
    private float rotationY;

    [Header("Mode suivi")]
    private bool followMode = false;
    private Transform followTarget;
    private Vector3 followOffset = Vector3.zero;
    private float followSmoothSpeed = 10f;
    private float followMinDistance = 0.5f;
    private float followMaxDistance = 3000f;
    private float followMinVerticalAngle = 5f;
    private float followMaxVerticalAngle = 175f;

    // Sauvegarde de l'état initial de la caméra
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private Transform savedParent;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;
    }

    void LateUpdate()
    {
        if (!followMode)
        {
            // Rotation de la caméra (clic droit maintenu)
            if (Input.GetMouseButton(1))
            {
                rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
                rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                rotationY = Mathf.Clamp(rotationY, -90f, 90f);

                transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
            }

            // Déplacement horizontal (Z/Q/S/D ou W/A/S/D)
            Vector3 direction = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );

            Vector3 displacement = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;
            transform.position += displacement;

            // Déplacement vertical (R/F)
            if (Input.GetKey(KeyCode.R))
                transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.F))
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;

            // Zoom avec la molette
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            transform.position += transform.forward * scroll * scrollSpeed;
        }
        else
        {
            // Mode suivi : rotation et zoom autour de la cible
            if (followTarget == null)
            {
                StopFollow();
                return;
            }

            // Zoom (ajustement de la distance au point cible)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 1e-6f)
            {
                float dist = followOffset.magnitude;
                dist = Mathf.Clamp(dist - scroll * scrollSpeed, followMinDistance, followMaxDistance);
                followOffset = followOffset.normalized * dist;
            }

            // Rotation autour de la cible (clic droit maintenu)
            if (Input.GetMouseButton(1))
            {
                float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
                float pitch = -Input.GetAxis("Mouse Y") * mouseSensitivity;

                followOffset = Quaternion.AngleAxis(yaw, Vector3.up) * followOffset;

                Vector3 right = Vector3.Cross(Vector3.up, followOffset).normalized;
                followOffset = Quaternion.AngleAxis(pitch, right) * followOffset;

                // Limite l'angle vertical pour éviter de passer sous la cible
                float angleFromUp = Vector3.Angle(followOffset, Vector3.up);
                float clamped = Mathf.Clamp(angleFromUp, followMinVerticalAngle, followMaxVerticalAngle);
                if (!Mathf.Approximately(angleFromUp, clamped))
                {
                    Vector3 horiz = Vector3.ProjectOnPlane(followOffset, Vector3.up).normalized;
                    float dist = followOffset.magnitude;
                    followOffset = Quaternion.AngleAxis(90f - clamped, right) * (horiz * dist);
                }
            }

            // Sort du mode suivi si une touche de déplacement est pressée
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Q) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A))
            {
                StopFollow();
                return;
            }

            // Lissage du déplacement vers la position cible
            Vector3 desiredPos = followTarget.position + followOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followSmoothSpeed * Time.deltaTime));

            // Oriente la caméra vers la cible
            transform.LookAt(followTarget.position);
        }
    }

    /// <summary>
    /// Active le mode suivi sur une cible donnée.
    /// </summary>
    public void StartFollow(
        Transform target,
        Vector3 worldOffset,
        float smoothSpeed = 10f,
        float minDistance = 0.5f,
        float maxDistance = 2000f,
        float rotateSpeed = 0.2f,
        float zoomSpeed = 50f,
        float minVertAngle = 5f,
        float maxVertAngle = 175f)
    {
        if (target == null) return;

        // Sauvegarde l'état actuel de la caméra
        savedPosition = transform.position;
        savedRotation = transform.rotation;
        savedParent = transform.parent;

        followTarget = target;
        followOffset = worldOffset;
        followSmoothSpeed = smoothSpeed;
        followMinDistance = minDistance;
        followMaxDistance = maxDistance;
        followMinVerticalAngle = minVertAngle;
        followMaxVerticalAngle = maxVertAngle;

        followMode = true;
    }

    /// <summary>
    /// Désactive le mode suivi et restaure la position d’origine.
    /// </summary>
    public void StopFollow()
    {
        if (!followMode) return;

        followMode = false;
        followTarget = null;

        transform.parent = savedParent;
        transform.position = savedPosition;
        transform.rotation = savedRotation;
    }

    public bool IsFollowing() => followMode;
}
