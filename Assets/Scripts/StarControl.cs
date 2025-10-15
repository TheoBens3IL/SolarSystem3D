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
    [SerializeField] private float starDiameter = 1.3927e9f;

    [Tooltip("Visual radius of the star (for rendering only, not physical value).")]
    [SerializeField] private float visualRadius = 1.0f;

    [Header("Debug")]
    [Tooltip("Display information about the star in the console.")]
    [SerializeField] private bool debugInfo = false;

    #endregion

    #region Private Fields

    private double gravitationalParameter; // ? = G * M

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Compute ? = G * M once at startup
        gravitationalParameter = PhysicsConstants.G * starMass;
        ApplyVisualScale();

        if (debugInfo)
            Debug.Log($"[StarControl] Initialized star: mass = {starMass:E2} kg, ? = {gravitationalParameter:E2}");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Automatically scales the visual representation of the star
    /// based on the scene's scale or distance compression factors.
    /// </summary>
    /// 
    private void ApplyVisualScale()
    {
        float scaledDiameter = VisualScale.ScaleRadius(starDiameter);
        transform.localScale = Vector3.one * scaledDiameter;
    }
    #endregion

    #region Public API

    /// <summary>
    /// Returns the gravitational parameter (? = G * M) used in Kepler’s laws.
    /// </summary>
    public double GetMu() => gravitationalParameter;

    /// <summary>
    /// Returns the current star mass in kilograms.
    /// </summary>
    public double GetMass() => starMass;

    /// <summary>
    /// Manually updates the star’s mass and recomputes ?.
    /// </summary>
    public void SetMass(double newMass)
    {
        starMass = newMass;
        gravitationalParameter = PhysicsConstants.G * starMass;
    }

    #endregion
}
