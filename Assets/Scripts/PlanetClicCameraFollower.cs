using System.Collections;
using UnityEngine;

/// <summary>
/// Permet de cliquer sur une planète pour que la caméra principale s'en approche puis la suive.
/// Appuyer sur Z / Q / S / D (ou W / A / S / D) quitte la vue suivie et rétablit la caméra.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlanetClicCameraFollower : MonoBehaviour
{
    [Tooltip("Durée (s) de la transition quand on se déplace vers la planète")]
    public float moveDuration = 1.0f;

    [Tooltip("Vitesse de suivi (position smoothing)")]
    public float followSmoothSpeed = 10f;

    [Tooltip("Offset local par rapport à la planète quand on suit (si zero -> conserve offset courant)")]
    public Vector3 followOffset = new Vector3(0f, 5f, -10f);

    // si true, on conserve l'offset courant de la caméra lors du clic (ignore followOffset)
    public bool keepCurrentOffsetOnSelect = false;

    private Camera mainCam;
    private Transform followTarget;
    private bool isFollowing = false;

    // sauvegarde de l'état caméra pour restauration
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private Transform savedParent;

    // offset actuel en world space (target.position + worldOffset)
    private Vector3 worldOffset;

    void Awake()
    {
        mainCam = Camera.main ?? GetComponent<Camera>();
        if (mainCam == null)
            Debug.LogError("[PlanetClickCameraFollower] Pas de Camera principale trouvée.");
    }

    void Update()
    {
        HandleClick();
        HandleExitInput();
    }

    void LateUpdate()
    {
        if (isFollowing && followTarget != null)
        {
            // position cible = target.position + worldOffset ; lissage
            Vector3 desiredPos = followTarget.position + worldOffset;
            mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, desiredPos, 1f - Mathf.Exp(-followSmoothSpeed * Time.deltaTime));

            // regarder la planète
            mainCam.transform.LookAt(followTarget.position);
        }
    }

    private void HandleClick()
    {
        if (Input.GetMouseButtonDown(0) == false) return;
        if (mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1e6f))
        {
            // chercher PlanetControl sur l'objet frappé ou sur ses parents
            var target = hit.collider.transform.GetComponentInParent<PlanetControl>();
            if (target != null)
            {
                StartCoroutine(MoveAndFollow(target.transform));
            }
        }
    }

    private void HandleExitInput()
    {
        // touches pour quitter le suivi : AZERTY (Z,Q,S,D) et QWERTY (W,A,S,D)
        if (isFollowing && (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Q) ||
                            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
                            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A)))
        {
            StopFollowing();
        }
    }

    private IEnumerator MoveAndFollow(Transform target)
    {
        if (mainCam == null || target == null) yield break;

        // sauvegarde état caméra
        savedPosition = mainCam.transform.position;
        savedRotation = mainCam.transform.rotation;
        savedParent = mainCam.transform.parent;

        followTarget = target;

        // calculer worldOffset
        if (keepCurrentOffsetOnSelect)
        {
            worldOffset = mainCam.transform.position - followTarget.position;
        }
        else
        {
            // convertir followOffset (local relative to target) en world offset
            worldOffset = followTarget.TransformVector(followOffset);
        }

        // calculer destination
        Vector3 dest = followTarget.position + worldOffset;

        // transition lissée
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

        isFollowing = true;
    }

    private void StopFollowing()
    {
        if (mainCam == null) return;

        isFollowing = false;
        followTarget = null;

        // restaurer état précédent
        mainCam.transform.parent = savedParent;
        mainCam.transform.position = savedPosition;
        mainCam.transform.rotation = savedRotation;
    }
}