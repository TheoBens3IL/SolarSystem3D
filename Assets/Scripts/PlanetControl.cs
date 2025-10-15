using System;
using UnityEngine;

#region Documentation
/// <summary>
/// PlanetControl
/// - Holds physical & visual parameters for a planet.
/// - Computes the planet position from Keplerian elements (real meters).
/// - Converts real positions to Unity display positions using ScaleDistance (logarithmic).
/// - Handles planet self-rotation (obliquity, retrograde/prograde).
/// 
/// Notes:
/// - Kepler calculations are done in double precision (via KeplerUtil).
/// - This script expects a StarControl in the scene with tag "Sun".
/// - Visual scaling (radius) uses VisualScale.ScaleRadius(...) for consistency.
/// </summary>
#endregion
[DisallowMultipleComponent]
public class PlanetControl : MonoBehaviour
{
    private enum RotationDirection { Prograde, Retrograde }

    #region Serialized Fields

    [Header("Identity & Physical")]
    [Tooltip("Planet display name (informational).")]
    [SerializeField] private string planetName = "Planet";

    [Tooltip("Mass in kilograms (physical).")]
    [SerializeField] private double planetMass = 5.972e24;

    [Tooltip("Physical diameter (meters).")]
    [SerializeField] private double planetDiameter = 1.2742e7;

    [Header("Keplerian Elements (SI)")]
    [Tooltip("Kepler elements are expected in SI units (a in m, angles in degrees).")]
    [SerializeField] private KeplerElements kepler = new KeplerElements();

    [Header("Rotation (spin)")]
    [SerializeField] private RotationDirection rotationDirection = RotationDirection.Prograde;
    [Tooltip("Rotation period in hours (planet day).")]
    [SerializeField] private double rotationPeriodHours = 24.0;
    [Tooltip("Visual speed multiplier for rotation.")]
    [SerializeField] private float rotationSpeedMultiplier = 1000f;
    [Tooltip("Obliquity (axial tilt) in degrees.")]
    [SerializeField] private float rotationObliquityDeg = 23.44f;
    [SerializeField] private bool enableSpin = true;

    [Tooltip("Distance scaling parameters (must match OrbitDrawer/others).")]
    [SerializeField] private float kDistance = 2000f;
    [SerializeField] private float alphaDistance = 1e-9f;

    #endregion

    #region Serializable Types

    [Serializable]
    public struct KeplerElements
    {
        [Tooltip("Semi-major axis (meters)")]
        public double a;
        [Tooltip("Eccentricity")]
        public double e;
        [Tooltip("Inclination (degrees)")]
        public double iDeg;
        [Tooltip("Longitude of ascending node (degrees)")]
        public double OmegaDeg;
        [Tooltip("Argument of periapsis (degrees)")]
        public double omegaDeg;
        [Tooltip("Mean anomaly at epoch (degrees)")]
        public double M0Deg;
        [Tooltip("Epoch time in seconds (reference for M0).")]
        public double epochSeconds;
    }

    #endregion

    #region Private Fields

    private GameObject sunObj;
    private StarControl sunScript;
    private double simulationTimeSeconds = 0.0;
    private double muCentral = PhysicsConstants.MuSun; // fallback
    private Vector3 spinAxisLocal = Vector3.up; // computed from obliquity

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        // compute local spin axis from obliquity (tilt around local X)
        ComputeSpinAxisFromObliquity();

        // Apply visual radius (uses VisualScale)
        ApplyVisualScale();
    }

    private void Start()
    {
        // cache the Sun and its mu
        sunObj = GameObject.FindGameObjectWithTag("Sun");
        if (sunObj != null)
        {
            sunScript = sunObj.GetComponent<StarControl>();
            if (sunScript != null)
            {
                muCentral = sunScript.GetMu();
            }
        }

        if (sunScript == null)
            Debug.LogError($"[PlanetControl:{planetName}] No Sun (StarControl) found - Kepler calculations will use fallback mu.");
    }

    private void Update()
    {
        // rotate on own axis (visual)
        if (enableSpin)
            SpinOnAxis();
    }

    private void FixedUpdate()
    {
        if (sunScript == null && GameObject.FindGameObjectWithTag("Sun") != null)
        {
            sunObj = GameObject.FindGameObjectWithTag("Sun");
            sunScript = sunObj?.GetComponent<StarControl>();
            if (sunScript != null) muCentral = sunScript.GetMu();
        }

        // --- UTILISATION DE SimulationClock ---
        double dtsinceEpoch = SimulationClock.Instance.SimulationTimeSeconds - kepler.epochSeconds;

        double a = kepler.a;
        double e = kepler.e;
        double iRad = kepler.iDeg * Mathf.Deg2Rad;
        double OmegaRad = kepler.OmegaDeg * Mathf.Deg2Rad;
        double omegaRad = kepler.omegaDeg * Mathf.Deg2Rad;
        double M0rad = kepler.M0Deg * Mathf.Deg2Rad;

        Vector3 posMeters = KeplerUtil.OrbitalElementsToPositionMeters(
            a, e, iRad, OmegaRad, omegaRad, M0rad, dtsinceEpoch, muCentral
        );

        float scaledUnityDistance = ScaleDistance(posMeters.magnitude);
        Vector3 dirUnity = posMeters.normalized;
        Vector3 sunPos = sunObj != null ? sunObj.transform.position : Vector3.zero;
        transform.position = sunPos + dirUnity * scaledUnityDistance;
    }


    #endregion

    #region Rotation / Spin

    /// <summary>
    /// Compute spin axis from obliquity value.
    /// We tilt the standard up vector by obliquity degrees around local X (can change convention if desired).
    /// </summary>
    private void ComputeSpinAxisFromObliquity()
    {
        spinAxisLocal = Quaternion.Euler((float)rotationObliquityDeg, 0f, 0f) * Vector3.up;
    }

    /// <summary>
    /// Apply incremental quaternion rotation so the spin accumulates (no reset each frame).
    /// </summary>
    private void SpinOnAxis()
    {
        if (rotationPeriodHours <= 0.0) return;

        // degrees per second (real)
        double degreesPerSecond = 360.0 / (rotationPeriodHours * 3600.0);
        float rotationThisFrame = (float)(degreesPerSecond * rotationSpeedMultiplier * Time.deltaTime);

        Vector3 axis = rotationDirection == RotationDirection.Prograde ? spinAxisLocal : -spinAxisLocal;

        // accumulate rotation using quaternion multiplication
        transform.localRotation *= Quaternion.AngleAxis(rotationThisFrame, axis);
    }

    #endregion

    #region Visual Scaling

    /// <summary>
    /// Apply visual scaling to the planet's transform using VisualScale utility.
    /// </summary>
    private void ApplyVisualScale()
    {
        float scaledDiameter = VisualScale.ScaleRadius((float)planetDiameter);
        transform.localScale = Vector3.one * scaledDiameter;
    }

    /// <summary>
    /// Convert a real distance (meters) to compressed Unity distance (logarithmic).
    /// Must be consistent with OrbitDrawer and other systems.
    /// </summary>
    private float ScaleDistance(float realDistance)
    {
        return kDistance * Mathf.Log(1f + alphaDistance * realDistance);
    }

    #endregion

    #region Public API

    public void SetKeplerElements(KeplerElements elements)
    {
        kepler = elements;
    }

    public KeplerElements GetKeplerElements() => kepler;

    public void SetPlanetDiameter(double diameterMeters)
    {
        planetDiameter = diameterMeters;
        ApplyVisualScale();
    }

    public double GetPlanetMass() => planetMass;
    public float GetKDistance() => kDistance;
    public float GetAlphaDistance() => alphaDistance;

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Make sure our visible representation updates in the editor
        ComputeSpinAxisFromObliquity();
        ApplyVisualScale();
    }
#endif

    #endregion
}
