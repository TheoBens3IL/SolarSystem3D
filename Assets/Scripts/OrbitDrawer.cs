using UnityEngine;
using System;

/// <summary>
/// OrbitDrawer: Generates and visualizes the orbital path of a planet
/// using Keplerian elements. Compatible with the logarithmic distance scaling
/// used in PlanetControl. The orbit is displayed via a LineRenderer.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class OrbitDrawer : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("Reference to the Sun transform (center of the system).")]
    [SerializeField] private Transform sunTransform;

    [Header("Keplerian Elements (SI Units)")]
    [Tooltip("Semi-major axis (m).")]
    [SerializeField] private double semiMajorAxis = 1.495978707e11; // 1 AU
    [Tooltip("Orbital eccentricity (dimensionless).")]
    [SerializeField] private double eccentricity = 0.0167;
    [Tooltip("Orbital inclination (degrees).")]
    [SerializeField] private double inclinationDeg = 0.0;
    [Tooltip("Longitude of ascending node (degrees).")]
    [SerializeField] private double longAscNodeDeg = 0.0;
    [Tooltip("Argument of periapsis (degrees).")]
    [SerializeField] private double argPeriapsisDeg = 0.0;

    [Header("Drawing Settings")]
    [Tooltip("Number of points used to draw the orbit.")]
    [SerializeField] private int resolution = 500;
    [Tooltip("Color of the orbit line.")]
    [SerializeField] private Color orbitColor = Color.white;
    [Tooltip("Width of the orbit line in Unity units.")]
    [SerializeField] private float lineWidth = 0.02f;

    [Header("Scaling Settings")]
    [Tooltip("Distance scaling constants. Must match PlanetControl.")]
    [SerializeField] private float kDistance = 2000f;
    [SerializeField] private float alphaDistance = 1e-9f;

    #endregion

    #region Private Fields

    private LineRenderer lineRenderer;
    private const double MU_SUN = 1.32712440018e20; // Gravitational parameter of the Sun (m^3/s^2)

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    private void Start()
    {
        if (sunTransform == null)
        {
            Debug.LogError("[OrbitDrawer] Missing reference to Sun transform.");
            return;
        }

        DrawOrbit();
    }

    #endregion

    #region LineRenderer Setup

    /// <summary>
    /// Configures the LineRenderer visual properties.
    /// </summary>
    private void SetupLineRenderer()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = lineWidth;

        // Simple unlit material for stable color rendering
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = orbitColor;
    }

    #endregion

    #region Orbit Drawing

    /// <summary>
    /// Generates and draws the orbital path using the planet's Keplerian parameters.
    /// </summary>
    private void DrawOrbit()
    {
        Vector3[] points = new Vector3[resolution];

        // Convert angles to radians
        double iRad = inclinationDeg * Mathf.Deg2Rad;
        double OmegaRad = longAscNodeDeg * Mathf.Deg2Rad;
        double omegaRad = argPeriapsisDeg * Mathf.Deg2Rad;

        for (int j = 0; j < resolution; j++)
        {
            double trueAnomaly = (j / (double)(resolution - 1)) * 2.0 * Math.PI;

            // Distance from focus (Sun) at current true anomaly
            double r = semiMajorAxis * (1 - eccentricity * eccentricity) /
                       (1 + eccentricity * Math.Cos(trueAnomaly));

            // Orbital plane coordinates
            double xOrb = r * Math.Cos(trueAnomaly);
            double yOrb = r * Math.Sin(trueAnomaly);
            double zOrb = 0;

            // Rotation matrices: Rz(Omega) * Rx(i) * Rz(omega)
            double cosO = Math.Cos(OmegaRad);
            double sinO = Math.Sin(OmegaRad);
            double cosi = Math.Cos(iRad);
            double sini = Math.Sin(iRad);
            double cosw = Math.Cos(omegaRad);
            double sinw = Math.Sin(omegaRad);

            // Step 1: rotation by ω (argument of periapsis)
            double x1 = cosw * xOrb - sinw * yOrb;
            double y1 = sinw * xOrb + cosw * yOrb;
            double z1 = zOrb;

            // Step 2: rotation by i (inclination)
            double x2 = x1;
            double y2 = cosi * y1 - sini * z1;
            double z2 = sini * y1 + cosi * z1;

            // Step 3: rotation by Ω (longitude of ascending node)
            double x = cosO * x2 - sinO * y2;
            double y = sinO * x2 + cosO * y2;
            double z = z2;

            // Apply distance scaling (logarithmic, consistent with PlanetControl)
            float scaledDist = ScaleDistance((float)Math.Sqrt(x * x + y * y + z * z));
            Vector3 dir = new Vector3((float)x, (float)z, (float)y).normalized;

            points[j] = sunTransform.position + dir * scaledDist;
        }

        lineRenderer.positionCount = resolution;
        lineRenderer.SetPositions(points);
    }

    #endregion

    #region Scaling Methods

    /// <summary>
    /// Applies the same logarithmic distance scaling used in PlanetControl.
    /// </summary>
    private float ScaleDistance(float realDistance)
    {
        return kDistance * Mathf.Log(1f + alphaDistance * realDistance);
    }

    #endregion
}
