using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitDrawer : MonoBehaviour
{
    [Header("Target Planet")]
    [Tooltip("Le PlanetControl dont on dessine l'orbite.")]
    public PlanetControl targetPlanet;

    [Header("Orbit Settings")]
    [Tooltip("Nombre de segments pour dessiner l'orbite.")]
    [Range(10, 1000)]
    public int segments = 360;

    [Tooltip("Largeur de la ligne au début de l'orbite.")]
    public float lineWidthStart = 0.05f;

    [Tooltip("Largeur de la ligne à la fin de l'orbite.")]
    public float lineWidthEnd = 0.05f;

    [Tooltip("Couleur de la ligne.")]
    public Color lineColor = Color.white;

    [Tooltip("Mettre à jour l'orbite en temps réel (peut impacter les performances).")]
    public bool updateEveryFrame = false;

    private LineRenderer lineRenderer;
    private float kDistance;
    private float alphaDistance;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;

        // Appliquer largeur et couleur
        lineRenderer.startWidth = lineWidthStart;
        lineRenderer.endWidth = lineWidthEnd;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = lineColor;
    }

    private void Start()
    {
        if (targetPlanet == null)
        {
            Debug.LogError("[OrbitDrawer] Aucun PlanetControl assigné !");
            return;
        }

        // Récupère kDistance et alphaDistance depuis PlanetControl
        kDistance = targetPlanet.GetKDistance();
        alphaDistance = targetPlanet.GetAlphaDistance();

        DrawOrbit();
    }

    private void Update()
    {
        if (updateEveryFrame)
            DrawOrbit();
    }

    public void DrawOrbit()
    {
        if (targetPlanet == null) return;

        var kepler = targetPlanet.GetKeplerElements();
        Vector3 sunPos = GameObject.FindGameObjectWithTag("Sun")?.transform.position ?? Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            // Calcul de l'anomalie moyenne sur un tour complet
            double M = 2.0 * Math.PI * i / segments;
            double posSeconds = M * Math.Sqrt(Math.Pow(kepler.a, 3) / PhysicsConstants.MuSun); // approximation

            Vector3 posMeters = KeplerUtil.OrbitalElementsToPositionMeters(
                kepler.a, kepler.e, kepler.iDeg * Mathf.Deg2Rad,
                kepler.OmegaDeg * Mathf.Deg2Rad,
                kepler.omegaDeg * Mathf.Deg2Rad,
                kepler.M0Deg * Mathf.Deg2Rad,
                posSeconds, PhysicsConstants.MuSun
            );

            float scaledDistance = kDistance * Mathf.Log(1f + alphaDistance * posMeters.magnitude);
            Vector3 dir = posMeters.normalized;

            lineRenderer.SetPosition(i, sunPos + dir * scaledDistance);
        }
    }
}
