using UnityEngine;

#region Documentation
/// <summary>
/// Controls the central star (e.g., the Sun) in the solar system simulation.
/// Responsible for holding the star’s physical parameters such as mass and radius,
/// and for providing gravitational data to orbiting bodies.
/// </summary>
#endregion
[DisallowMultipleComponent]
public class StarControl : MonoBehaviour
{
    #region Serialized Fields

    [Header("Star Settings")]
    [Tooltip("Mass of the star in kilograms.")]
    [SerializeField] private double starMass = PhysicsConstants.MassSun;

    [Tooltip("Real physical diameter of the star (meters).")]
    [SerializeField] private float starDiameter = 1.3927e9f; // Sun ~ 1,392,700 km

    [Header("Visual Settings")]
    [Tooltip("Visual scale multiplier for artistic size adjustment (1 = realistic).")]
    [SerializeField, Range(0.1f, 100f)] private float visualScaleMultiplier = 3f;

    [Tooltip("Enable automatic scaling of the star visual size.")]
    [SerializeField] private bool autoScale = true;

    [Header("Debug")]
    [Tooltip("Display information about the star in the console.")]
    [SerializeField] private bool debugInfo = false;

    #endregion

    #region Private Fields

    private double gravitationalParameter; // μ = G * M

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Compute μ = G * M once at startup
        gravitationalParameter = PhysicsConstants.G * starMass;

        if (autoScale)
            ApplyVisualScale();

        if (debugInfo)
            Debug.Log($"[StarControl] Initialized star: mass = {starMass:E2} kg, μ = {gravitationalParameter:E2}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Refresh the scale instantly in editor when parameters change
        if (autoScale && Application.isEditor)
            ApplyVisualScale();
    }
#endif

    #endregion

    #region Private Methods

    /// <summary>
    /// Automatically scales the visual representation of the star
    /// based on its diameter and the visual scale multiplier.
    /// </summary>
    private void ApplyVisualScale()
    {
        float scaledDiameter = VisualScale.ScaleRadius(starDiameter) * visualScaleMultiplier;
        transform.localScale = Vector3.one * scaledDiameter;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Returns the gravitational parameter (μ = G * M) used in Kepler’s laws.
    /// </summary>
    public double GetMu() => gravitationalParameter;

    /// <summary>
    /// Returns the current star mass in kilograms.
    /// </summary>
    public double GetMass() => starMass;

    /// <summary>
    /// Manually updates the star’s mass and recomputes μ.
    /// </summary>
    public void SetMass(double newMass)
    {
        starMass = newMass;
        gravitationalParameter = PhysicsConstants.G * starMass;
    }

    /// <summary>
    /// Forces a visual rescale of the star (can be called manually).
    /// </summary>
    public void RefreshScale()
    {
        ApplyVisualScale();
    }

    #endregion
}
