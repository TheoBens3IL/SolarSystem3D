using UnityEngine;

/// <summary>
/// ControlPlanet: Manages the physics setup of a planet object in a solar system.
/// Automatically adds a Rigidbody and a SphereCollider to the GameObject and 
/// allows you to configure the mass through the Inspector.
/// </summary>
[RequireComponent(typeof(Transform))] // Ensure Transform is present (always true)
public class ControlPlanet : MonoBehaviour
{
    /// <summary>
    /// Defines the rotation direction: Prograde (default) or Retrograde (opposite).
    /// </summary>
    private enum RotationDirection { Prograde, Retrograde }

    #region Serialized Fields
    [Header("Planet Settings")]
    [Tooltip("Mass of the planet in kilograms. This value will be applied to the Rigidbody.")]
    [SerializeField] private float planetMass = 5.972e24f; // Default: Earth mass

    [Header("Position Settings")]
    [Tooltip("The reference object (for example the Sun) from which the distance is measured.")]
    [SerializeField] private Transform referenceObject;

    [Tooltip("Distance from the reference object in meters.")]
    [SerializeField] private float distanceFromReference = 1.5e11f; // ~1 AU in meters

    [Tooltip("Direction vector from the reference object. Defaults to X axis.")]
    [SerializeField] private Vector3 directionFromReference = Vector3.right;

    [Header("Rotation Settings")]
    [Tooltip("Rotation direction (Prograde = normal, Retrograde = opposite).")]
    [SerializeField] private RotationDirection rotationDirection = RotationDirection.Prograde;

    [Tooltip("Duration of one day on this planet in hours (Earth = 24).")]
    [SerializeField] private float rotationPeriodHours = 24f;

    [Tooltip("Speed multiplier to make rotation visible in-game.")]
    [SerializeField] private float rotationSpeedMultiplier = 1000f;

    [Tooltip("Axis around which the planet rotates (default Y-axis).")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    #endregion

    #region Private Components
    private Rigidbody planetRigidbody;
    #endregion

    #region Unity Callbacks
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// We use it to ensure the components are set up before the game starts.
    /// </summary>
    private void Awake()
    {
        SetupComponents();
        ApplyMass();
    }

    private void Update()
    {
        RotatePlanet();
    }

    #endregion

    #region Setup Methods
    /// <summary>
    /// Ensures that the GameObject has a Rigidbody and a SphereCollider.
    /// If not present, they will be added automatically.
    /// </summary>
    private void SetupComponents()
    {
        // Get or add Rigidbody
        planetRigidbody = GetComponent<Rigidbody>();
        if (planetRigidbody == null)
        {
            planetRigidbody = gameObject.AddComponent<Rigidbody>();
            Debug.Log($"[{nameof(ControlPlanet)}] Added Rigidbody to {gameObject.name}.");
        }

        // Optional: Make the Rigidbody non-kinematic so physics works
        planetRigidbody.useGravity = false; // Disable default gravity
        planetRigidbody.isKinematic = false;
    }

    /// <summary>
    /// Applies the mass defined in the Inspector to the Rigidbody.
    /// </summary>
    private void ApplyMass()
    {
        // Unity Rigidbody mass is in kilograms by default
        planetRigidbody.mass = planetMass;
    }

    /// <summary>
    /// Places the planet at the defined distance from the reference object.
    /// </summary>
    private void PlacePlanet()
    {
        // Safety check
        if (referenceObject == null) return;

        // Normalize direction to avoid scaling errors
        Vector3 dir = directionFromReference.normalized;

        // Calculate the new position based on distance and direction
        Vector3 newPos = referenceObject.position + dir * distanceFromReference;

        // Apply position to this planet
        transform.position = newPos;
    }

    #region Rotation
    /// <summary>
    /// Rotates the planet around its own axis based on real rotation period,
    /// speed multiplier, and rotation direction.
    /// </summary>
    private void RotatePlanet()
    {
        if (rotationPeriodHours <= 0f) return;

        // Calculate degrees per real-time second for one full rotation
        float degreesPerSecond = 360f / (rotationPeriodHours * 3600f);

        // Apply speed multiplier to accelerate rotation for visibility
        float rotationThisFrame = degreesPerSecond * rotationSpeedMultiplier * Time.deltaTime;

        // Determine axis sign based on rotation direction
        Vector3 axis = (rotationDirection == RotationDirection.Prograde)
            ? rotationAxis
            : -rotationAxis;

        // Rotate around axis
        transform.Rotate(axis, rotationThisFrame, Space.Self);
    }
    #endregion
    #endregion

    #region Public API
    /// <summary>
    /// Allows other scripts to update the planet's mass at runtime safely.
    /// </summary>
    /// <param name="newMass">New mass value in kilograms.</param>
    public void SetPlanetMass(float newMass)
    {
        planetMass = Mathf.Max(newMass, 0f); // Ensure no negative mass
        ApplyMass();
    }

    /// <summary>
    /// Returns the current mass of the planet.
    /// </summary>
    public float GetPlanetMass()
    {
        return planetMass;
    }

    /// <summary>
    /// Allows other scripts to update the distance from the reference object at runtime.
    /// </summary>
    public void SetDistanceFromReference(float newDistance)
    {
        distanceFromReference = Mathf.Max(newDistance, 0f);
        PlacePlanet();
    }

    /// <summary>
    /// Returns the current distance from the reference object.
    /// </summary>
    public float GetDistanceFromReference()
    {
        return distanceFromReference;
    }
    #endregion
}
