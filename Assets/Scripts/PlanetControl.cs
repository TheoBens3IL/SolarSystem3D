using UnityEngine;

/// <summary>
/// ControlPlanet: Manages the physics setup of a planet object in a solar system.
/// Handles scaling (visual vs real) and physics with real values.
/// </summary>
[RequireComponent(typeof(Transform))]
public class PlanetControl : MonoBehaviour
{
    private enum RotationDirection { Prograde, Retrograde }

    #region Serialized Fields
    [Header("Planet Settings")]
    [Tooltip("Mass of the planet in kilograms.")]
    [SerializeField] private float planetMass = 5.972e24f; // Default Earth

    [Header("Size Settings")]
    [Tooltip("Real diameter of the planet in meters.")]
    [SerializeField] private float planetDiameter = 1.2742e7f; // Default Earth (12,742 km)

    [Header("Rotation Settings")]
    [SerializeField] private RotationDirection rotationDirection = RotationDirection.Prograde;
    [SerializeField] private float rotationPeriodHours = 24f;
    [SerializeField] private float rotationSpeedMultiplier = 1000f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Orbital Settings")]
    [SerializeField] private float initialDistance = 1.5e11f; // meters (~1 AU)
    [SerializeField] private Vector3 initialVelocity = new Vector3(0f, 0f, 29780f);

    [Header("Time Settings")]
    [SerializeField] private float timeScale = 100000f;

    [Header("Scaling Settings")]
    [Tooltip("Global scale factor for distances.")]
    [SerializeField] private float kDistance = 2000f;
    [SerializeField] private float alphaDistance = 1e-9f;
    [Tooltip("Global scale factors for radius.")]
    [SerializeField] private float betaRadius = 0.1f;
    [SerializeField] private float gammaRadius = 0.5f;
    #endregion

    #region Private Fields
    private Rigidbody planetRigidbody;
    private Vector3 velocity;
    private Vector3 acceleration;
    private const float G = 6.67430e-11f; // gravitational constant
    private StarControl sunScript;
    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        SetupComponents();
        ApplyMass();
        ApplyVisualScale();
    }

    private void Start()
    {
        // Récupère le Soleil ici, Start() est appelé après tous les Awake()
        GameObject sunObj = GameObject.FindGameObjectWithTag("Sun");
        if (sunObj != null)
            sunScript = sunObj.GetComponent<StarControl>();

        if (sunScript == null)
        {
            Debug.LogError("[PlanetControl] No Sun (StarControl) found!");
            return;
        }

        // Initialise la position de la planète par rapport au Soleil
        InitializeOrbit();
    }


    private void Update()
    {
        RotatePlanet();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime * timeScale;
        //VerletStep(dt);
    }
    #endregion

    #region Setup Methods
    private void SetupComponents()
    {
        planetRigidbody = GetComponent<Rigidbody>();
        if (planetRigidbody == null)
        {
            planetRigidbody = gameObject.AddComponent<Rigidbody>();
            Debug.Log($"[{nameof(PlanetControl)}] Added Rigidbody to {gameObject.name}.");
        }
        planetRigidbody.useGravity = false;
        planetRigidbody.isKinematic = true;
    }

    private void ApplyMass()
    {
        planetRigidbody.mass = planetMass;
    }

    /// <summary>
    /// Applies the scaled diameter to the planet's transform so it's visible in Unity.
    /// </summary>
    private void ApplyVisualScale()
    {
        // Conversion du diamètre réel en échelle Unity
        float scaledDiameter = ScaleRadius(planetDiameter);

        // Unity utilise "localScale" comme facteur multiplicatif sur un Mesh par défaut de taille 1.
        // Donc on met directement le diamètre échelle Unity.
        transform.localScale = new Vector3(scaledDiameter, scaledDiameter, scaledDiameter);
    }

    /// <summary>
    /// Initialise la planète sur l'axe X à la distance définie (échelle compressée).
    /// </summary>
    private void InitializeOrbit()
    {
        if (sunScript == null)
        {
            Debug.LogError("[ControlPlanet] No Sun (ControlStar) found!");
            return;
        }

        // Position initiale : distance compressée sur l'axe X à partir du Soleil
        float scaledInitialDistance = ScaleDistance(initialDistance);
        transform.position = sunScript.transform.position + Vector3.right * scaledInitialDistance;

        // Vitesse initiale et accélération
        //velocity = initialVelocity;
        //acceleration = ComputeAcceleration(transform.position);
    }
    #endregion

    #region Rotation
    private void RotatePlanet()
    {
        if (rotationPeriodHours <= 0f) return;

        float degreesPerSecond = 360f / (rotationPeriodHours * 3600f);
        float rotationThisFrame = degreesPerSecond * rotationSpeedMultiplier * Time.deltaTime;

        Vector3 axis = (rotationDirection == RotationDirection.Prograde)
            ? rotationAxis
            : -rotationAxis;

        transform.Rotate(axis, rotationThisFrame, Space.Self);
    }
    #endregion

    #region Verlet Integration
    private void VerletStep(float dt)
    {
        Vector3 currentPosition = transform.position;
        Vector3 newPosition = currentPosition + velocity * dt + 0.5f * acceleration * dt * dt;

        Vector3 newAcceleration = ComputeAcceleration(newPosition);

        velocity += 0.5f * (acceleration + newAcceleration) * dt;

        transform.position = newPosition;
        acceleration = newAcceleration;
    }

    private Vector3 ComputeAcceleration(Vector3 planetPosScaled)
    {
        if (sunScript == null) return Vector3.zero;

        // direction vers le Soleil
        Vector3 direction = sunScript.transform.position - planetPosScaled;

        // distance réelle
        float dScaled = direction.magnitude;
        float dReal = InverseScaleDistance(dScaled);

        if (dReal < 1e-3f) return Vector3.zero;

        float refMass = sunScript.GetMass();

        float accelMag = G * refMass / (dReal * dReal);

        return direction.normalized * accelMag;
    }
    #endregion

    #region Scaling
    private float ScaleDistance(float d) =>
        kDistance * Mathf.Log(1f + alphaDistance * d);

    private float InverseScaleDistance(float dScaled) =>
        (Mathf.Exp(dScaled / kDistance) - 1f) / alphaDistance;

    private float ScaleRadius(float r) =>
        betaRadius * Mathf.Pow(r, gammaRadius);

    private float InverseScaleRadius(float rScaled) =>
        Mathf.Pow(rScaled / betaRadius, 1f / gammaRadius);
    #endregion

    #region Getter & Setter
    public void SetPlanetMass(float newMass)
    {
        planetMass = Mathf.Max(newMass, 0f);
        ApplyMass();
    }

    public float GetPlanetMass() => planetMass;

    // Getter / Setter pour initialVelocity (réel, en m/s)
    public void SetInitialVelocity(Vector3 newInitialVelocity)
    {
        initialVelocity = newInitialVelocity;
        velocity = newInitialVelocity;
    }

    public Vector3 GetInitialVelocity() => initialVelocity;

    // Accès au timeScale (utilisé pour convertir Time.fixedDeltaTime en "dt réel")
    public float GetTimeScale() => timeScale;

    public void SetTimeScale(float newTimeScale)
    {
        timeScale = Mathf.Max(newTimeScale, 0f);
    }

    public float GetKDistance() => kDistance;
    public float GetAlphaDistance() => alphaDistance;

    #endregion
}
