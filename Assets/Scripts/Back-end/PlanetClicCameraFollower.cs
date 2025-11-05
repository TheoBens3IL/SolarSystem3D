using System.Collections;
using UnityEngine;

/// <summary>
/// Gère le clic sur une planète pour que la caméra principale s’en approche
/// puis la suive automatiquement.  
/// 
/// Le joueur peut quitter la vue suivie à tout moment avec Z/Q/S/D ou W/A/S/D.
/// Le véritable suivi est délégué au script ControllCamera pour éviter les doublons.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlanetClicCameraFollower : MonoBehaviour
{
    [Header("Interface d'information")]
    public PlanetInfoPanel infoPanel;

    [Header("Transition")]
    [Tooltip("Durée de la transition vers la planète (en secondes).")]
    public float moveDuration = 1.0f;

    [Header("Paramètres de suivi")]
    [Tooltip("Vitesse de lissage du mouvement pendant le suivi.")]
    public float followSmoothSpeed = 10f;

    [Tooltip("Décalage local appliqué à la caméra pendant le suivi (si zéro, conserve l’offset courant).")]
    public Vector3 followOffset = new Vector3(0f, 5f, -10f);

    [Tooltip("Si activé, conserve l’offset actuel de la caméra au moment du clic.")]
    public bool keepCurrentOffsetOnSelect = false;

    [Tooltip("Distance minimale entre la caméra et la planète pendant le suivi.")]
    public float followMinDistance = 0.5f;

    [Tooltip("Distance maximale de suivi.")]
    public float followMaxDistance = 2000f;

    [Tooltip("Vitesse de rotation de la caméra en mode suivi.")]
    public float followRotateSpeed = 0.2f;

    [Tooltip("Vitesse du zoom pendant le suivi.")]
    public float followZoomSpeed = 50f;

    [Tooltip("Angle vertical minimum autorisé.")]
    public float followMinVerticalAngle = 5f;

    [Tooltip("Angle vertical maximum autorisé.")]
    public float followMaxVerticalAngle = 175f;

    private Camera mainCam;
    private ControllCamera controllCam;
    private Transform followTarget;

    private void Awake()
    {
        mainCam = Camera.main ?? GetComponent<Camera>();
        controllCam = mainCam != null ? mainCam.GetComponent<ControllCamera>() : null;

        if (mainCam == null)
            Debug.LogError("[PlanetClicCameraFollower] Aucune caméra principale trouvée.");
        if (controllCam == null)
            Debug.LogWarning("[PlanetClicCameraFollower] Composant ControllCamera non détecté. Le suivi utilisera un comportement simplifié.");
    }

    private void Update()
    {
        HandleClick();

        // Fallback : si aucune ControllCamera, ignorer les touches de sortie
        if (controllCam == null && Input.anyKeyDown)
        {
            // Rien à faire
        }
    }

    private void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0) || mainCam == null)
            return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1e6f))
        {
            var planet = hit.collider.transform.GetComponentInParent<PlanetControl>();
            if (planet != null)
            {
                StartCoroutine(MoveAndDelegateFollow(planet.transform));
            }
        }
    }

    private IEnumerator MoveAndDelegateFollow(Transform target)
    {
        if (mainCam == null || target == null) yield break;

        followTarget = target;

        // Détermination de l’offset en espace monde
        Vector3 worldOffset;
        if (keepCurrentOffsetOnSelect)
        {
            worldOffset = mainCam.transform.position - followTarget.position;
            if (worldOffset.magnitude < 1e-6f)
                worldOffset = followOffset;
        }
        else
        {
            worldOffset = followTarget.TransformVector(followOffset);
        }

        // Clamp de la distance
        float distance = Mathf.Clamp(worldOffset.magnitude, followMinDistance, followMaxDistance);
        worldOffset = worldOffset.normalized * distance;

        // Transition douce vers la position de suivi
        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;
        Vector3 targetPos = followTarget.position + worldOffset;
        Quaternion targetRot = Quaternion.LookRotation(followTarget.position - targetPos, Vector3.up);

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));

            mainCam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            if (target != null && infoPanel != null)
                infoPanel.ShowInfo(target.GetComponent<PlanetControl>());

            yield return null;
        }

        // Position finale garantie
        mainCam.transform.position = targetPos;
        mainCam.transform.rotation = targetRot;

        // Délégation du suivi à ControllCamera si disponible
        if (controllCam != null)
        {
            controllCam.StartFollow(
                followTarget, worldOffset,
                followSmoothSpeed, followMinDistance, followMaxDistance,
                followRotateSpeed, followZoomSpeed,
                followMinVerticalAngle, followMaxVerticalAngle
            );
        }
    }

    /// <summary>
    /// Arrête le suivi en cours si une ControllCamera est active.
    /// </summary>
    public void StopFollowing()
    {
        if (controllCam != null)
            controllCam.StopFollow();
    }

    /// <summary>
    /// Permet de sélectionner une planète depuis l’UI.
    /// </summary>
    public void SelectPlanetFromUI(PlanetControl planet)
    {
        if (planet != null)
            StartCoroutine(MoveAndDelegateFollow(planet.transform));
    }
}
