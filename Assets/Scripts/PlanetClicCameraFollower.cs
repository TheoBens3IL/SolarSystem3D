using System.Collections;
using UnityEngine;

/// <summary>
/// Permet de cliquer sur une planète pour que la caméra principale s'en approche puis la suive.
/// Appuyer sur Z / Q / S / D (ou W / A / S / D) quitte la vue suivie et rétablit la caméra.
/// Ce script sélectionne la planète et délègue le suivi au ControllCamera (évite duplication).
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlanetClicCameraFollower : MonoBehaviour
{
    [Tooltip("Durée (s) de la transition quand on se déplace vers la planète")]
    public float moveDuration = 1.0f;

    [Tooltip("Vitesse de suivi (position smoothing) transférée à la ControllCamera")]
    public float followSmoothSpeed = 10f;

    [Tooltip("Offset local par rapport à la planète quand on suit (si zero -> conserve offset courant)")]
    public Vector3 followOffset = new Vector3(0f, 5f, -10f);

    // si true, on conserve l'offset courant de la caméra lors du clic (ignore followOffset)
    public bool keepCurrentOffsetOnSelect = false;

    // paramètres transférés à ControllCamera
    public float followMinDistance = 0.5f;
    public float followMaxDistance = 2000f;
    public float followRotateSpeed = 0.2f;
    public float followZoomSpeed = 50f;
    public float followMinVerticalAngle = 5f;
    public float followMaxVerticalAngle = 175f;

    private Camera mainCam;
    private ControllCamera controllCam;
    private Transform followTarget;

    void Awake()
    {
        mainCam = Camera.main ?? GetComponent<Camera>();
        controllCam = mainCam != null ? mainCam.GetComponent<ControllCamera>() : null;
        if (mainCam == null)
            Debug.LogError("[PlanetClickCameraFollower] Pas de Camera principale trouvée.");
        if (controllCam == null)
            Debug.LogWarning("[PlanetClickCameraFollower] ControllCamera non trouvé sur la caméra principale. Le suivi utilisera un comportement minimal.");
    }

    void Update()
    {
        HandleClick();
        // exit handled by ControllCamera if in follow mode, but keep StopFollowing fallback
        if (controllCam == null && Input.anyKeyDown)
        {
            // nothing
        }
    }

    private void HandleClick()
    {
        if (Input.GetMouseButtonDown(0) == false) return;
        if (mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1e6f))
        {
            var target = hit.collider.transform.GetComponentInParent<PlanetControl>();
            if (target != null)
            {
                StartCoroutine(MoveAndDelegateFollow(target.transform));
            }
        }
    }

    private IEnumerator MoveAndDelegateFollow(Transform target)
    {
        if (mainCam == null || target == null) yield break;

        followTarget = target;

        // calculer worldOffset
        Vector3 worldOffset;
        if (keepCurrentOffsetOnSelect)
        {
            worldOffset = mainCam.transform.position - followTarget.position;
            if (worldOffset.magnitude < 1e-6f) worldOffset = followOffset;
        }
        else
        {
            worldOffset = followTarget.TransformVector(followOffset);
        }

        // clamp distance
        float d = Mathf.Clamp(worldOffset.magnitude, followMinDistance, followMaxDistance);
        worldOffset = worldOffset.normalized * d;

        // transition lissée vers la position initiale de follow
        Vector3 dest = followTarget.position + worldOffset;
        float elapsed = 0f;
        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;
        Quaternion endRot = Quaternion.LookRotation(followTarget.position - dest, Vector3.up);

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
            mainCam.transform.position = Vector3.Lerp(startPos, dest, t);
            mainCam.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // garantir position finale
        mainCam.transform.position = dest;
        mainCam.transform.rotation = endRot;

        // déléguer le suivi à ControllCamera si présent
        if (controllCam != null)
        {
            controllCam.StartFollow(followTarget, worldOffset,
                followSmoothSpeed, followMinDistance, followMaxDistance,
                followRotateSpeed, followZoomSpeed, followMinVerticalAngle, followMaxVerticalAngle);
        }
    }

    // Optionnel : si ControllCamera absent, on peut implémenter un Stop fallback
    public void StopFollowing()
    {
        if (controllCam != null)
        {
            controllCam.StopFollow();
        }
    }
}