using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitDrawer : MonoBehaviour
{
    [Header("Target Planet")]
    [Tooltip("Le PlanetControl dont on dessine l'orbite.")]
    public PlanetControl targetPlanet;

    [Header("Orbit Settings")]
    [Range(10, 1000)] public int segments = 360;
    public float lineWidthStart = 0.05f;
    public float lineWidthEnd = 0.05f;
    public Color lineColor = Color.white;
    public bool updateEveryFrame = false;

    private LineRenderer lineRenderer;
    private float kDistance;
    private float alphaDistance;
    private bool orbitReady = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;

        // Line style
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

        // S’abonne à l’événement du PlanetControl
        targetPlanet.OnKeplerElementsUpdated += OnPlanetKeplerReady;

        // Si la planète est déjà initialisée (cas rare mais utile)
        TryInitialize();
    }

    private void OnDestroy()
    {
        if (targetPlanet != null)
            targetPlanet.OnKeplerElementsUpdated -= OnPlanetKeplerReady;
    }

    private void OnPlanetKeplerReady(PlanetControl planet)
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        if (targetPlanet == null) return;

        kDistance = targetPlanet.GetKDistance();
        alphaDistance = targetPlanet.GetAlphaDistance();

        DrawOrbit();
        orbitReady = true;
    }

    private void Update()
    {
        if (updateEveryFrame && orbitReady)
            DrawOrbit();
    }

    public void DrawOrbit()
    {
        if (targetPlanet == null) return;

        var kepler = targetPlanet.GetKeplerElements();
        Vector3 sunPos = GameObject.FindGameObjectWithTag("Sun")?.transform.position ?? Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            double M = 2.0 * Math.PI * i / segments;
            double posSeconds = M * Math.Sqrt(Math.Pow(kepler.a, 3) / PhysicsConstants.MuSun);

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
