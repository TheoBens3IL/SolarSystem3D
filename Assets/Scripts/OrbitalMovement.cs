using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OrbitalMovement: Calcule les trajectoires réelles des planètes avec Verlet
/// et les affiche dans Unity en utilisant les facteurs d'échelle.
/// </summary>
public class OrbitalMovement : MonoBehaviour
{
    [Header("Simulation / Scaling")]
    public int pointsPerOrbit = 2000;

    [Header("Planètes à inclure")]
    public PlanetControl[] planets;

    private LineRenderer[] lineRenderers;
    private List<Vector3>[] trajectories;

    IEnumerator Start()
    {
        yield return null; // attendre que PlanetControl ait initialisé

        if (planets == null || planets.Length == 0)
            planets = FindObjectsByType<PlanetControl>(FindObjectsSortMode.None);

        int n = planets.Length;
        Debug.Log($"Nombre de planètes : {n}");

        trajectories = new List<Vector3>[n];
        lineRenderers = new LineRenderer[n];

        Camera mainCam = Camera.main;

        for (int i = 0; i < n; i++)
        {
            PlanetControl planet = planets[i];
            if (planet == null) continue;

            trajectories[i] = new List<Vector3>();

            // Calcul des positions réelles
            Vector3[] points = ComputeOrbit(planet, pointsPerOrbit);

            // Convertir pour affichage Unity avec l’échelle
            for (int p = 0; p < points.Length; p++)
            {
                Vector3 displayPos = points[p] * planet.GetAlphaDistance() * planet.GetKDistance();
                trajectories[i].Add(displayPos);
            }

            // Créer LineRenderer pour afficher l'orbite
            float widthWorld = 0.02f;
            if (mainCam != null)
                widthWorld = ComputeWidthWorld(mainCam, trajectories[i][0], 6f, 0.0001f, 10f);

            lineRenderers[i] = CreateOrbitLineRenderer("Orbit_" + planet.name, trajectories[i].ToArray(), widthWorld);
        }
    }

    /// <summary>
    /// Intègre la trajectoire de la planète avec Verlet.
    /// </summary>
    private Vector3[] ComputeOrbit(PlanetControl planet, int steps)
    {
        Vector3[] positions = new Vector3[steps];

        // Récupérer valeurs réelles
        Vector3 pos = planet.transform.position; // Initial position in meters
        Vector3 vel = planet.GetInitialVelocity();
        float mass = planet.GetPlanetMass();
        float dt = planet.GetTimeScale(); // dt réel en s (Time.fixedDeltaTime * timeScale)

        pos *= 1f / (planet.GetAlphaDistance() * planet.GetKDistance()); // reconversion inverse si nécessaire

        // Constante gravitationnelle
        const double G = 6.67430e-11;

        for (int step = 0; step < steps; step++)
        {
            // Calcul accélération due à toutes les autres planètes et au Soleil
            Vector3 acc = ComputeAcceleration(pos, planet, planets);

            // Verlet
            Vector3 posNext = pos + vel * dt + 0.5f * acc * dt * dt;
            Vector3 accNext = ComputeAcceleration(posNext, planet, planets);
            vel += 0.5f * (acc + accNext) * dt;

            positions[step] = pos;

            // Logs pour debug
            if (step % 200 == 0)
                Debug.Log($"[{planet.name}] Step {step}: pos={pos}, acc={acc}, posNext={posNext}");

            pos = posNext;
        }

        return positions;
    }

    /// <summary>
    /// Calcul l'accélération gravitationnelle exercée sur une planète par le Soleil et les autres planètes.
    /// </summary>
    private Vector3 ComputeAcceleration(Vector3 pos, PlanetControl planet, PlanetControl[] allPlanets)
    {
        const double G = 6.67430e-11;
        Vector3 acc = Vector3.zero;

        // Attraction du Soleil (position 0,0,0)
        Vector3 toSun = -pos;
        double r2 = toSun.sqrMagnitude + 1e-9; // éviter div0
        acc += (float)(G * 1.9885e30 / r2) * toSun.normalized; // masse du Soleil ~1.9885e30 kg

        // Attraction des autres planètes
        foreach (var other in allPlanets)
        {
            if (other == planet) continue;
            Vector3 diff = other.transform.position - pos;
            double dist2 = diff.sqrMagnitude + 1e-9;
            acc += (float)(G * other.GetPlanetMass() / dist2) * diff.normalized;
        }

        return acc;
    }

    private float ComputeWidthWorld(Camera cam, Vector3 planetPos, float targetPixels, float minWidthWorld, float maxWidthWorld)
    {
        float widthWorld = 0.02f;
        if (cam == null) return widthWorld;

        float distance = Vector3.Distance(cam.transform.position, planetPos);

        if (cam.orthographic)
        {
            float worldPerPixel = (cam.orthographicSize * 2f) / Screen.height;
            widthWorld = targetPixels * worldPerPixel;
        }
        else
        {
            float fovRad = cam.fieldOfView * Mathf.Deg2Rad;
            float worldHeightAtDist = 2f * distance * Mathf.Tan(fovRad * 0.5f);
            float worldPerPixel = worldHeightAtDist / Screen.height;
            widthWorld = targetPixels * worldPerPixel;
        }

        return Mathf.Clamp(widthWorld, minWidthWorld, maxWidthWorld);
    }

    private LineRenderer CreateOrbitLineRenderer(string name, Vector3[] points, float widthWorld)
    {
        GameObject lrObj = new GameObject(name);
        lrObj.transform.parent = transform;
        var lr = lrObj.AddComponent<LineRenderer>();

        lr.positionCount = points.Length;
        lr.widthCurve = AnimationCurve.Constant(0, 1, 1f);
        lr.widthMultiplier = 1f;
        lr.startWidth = widthWorld;
        lr.endWidth = widthWorld;
        lr.numCornerVertices = 8;
        lr.numCapVertices = 8;
        lr.alignment = LineAlignment.View;
        lr.textureMode = LineTextureMode.Stretch;
        lr.useWorldSpace = true;
        lr.loop = true;

        Shader s = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
        lr.material = new Material(s);
        lr.material.SetColor("_Color", Color.white);
        lr.startColor = lr.endColor = Color.white;

        if (points.Length > 0)
            lr.SetPositions(points);

        return lr;
    }
}
