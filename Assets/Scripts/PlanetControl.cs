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
    [SerializeField] private float rotationObliquity = 23.5f; // degrees
    [SerializeField] private float precessionRate = 23.5f; // degree
    [SerializeField] private float precessionOffset = 23.5f; // degrees
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Kepler Elements (SI)")]
    [Tooltip("Semi-major axis in meters")]
    [SerializeField] private double keplerSemiMajorAxis = 1.495978707e11; // 1 AU in meters
    [SerializeField] private double keplerEccentricity = 0.0167;
    [SerializeField] private double keplerInclinationDeg = 0.0;
    [SerializeField] private double keplerLongAscNodeDeg = 0.0;
    [SerializeField] private double keplerArgPeriapsisDeg = 0.0;
    [Tooltip("Mean anomaly at epoch (degrees)")]
    [SerializeField] private double keplerMeanAnomalyDeg = 0.0;
    [Tooltip("Epoch time (seconds) as reference for M0. We'll start at t=0 by default.")]
    [SerializeField] private double keplerEpochSeconds = 0.0;


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
    //private Rigidbody planetRigidbody;
    //private Vector3 velocity;
    //private Vector3 acceleration;
    private const float G = 6.67430e-11f;// gravitational constant
    private GameObject sunObj;
    private StarControl sunScript;
    // internal simulated time (seconds)
    private double simulationTimeSeconds = 0.0;

    // central body gravitational parameter (mu = G * M_sun)
    private double muCentral = 6.67430e-11 * 1.98847e30; // default to Sun

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        //SetupComponents();
        ApplyVisualScale();

        // Calcule l’axe à partir de l’obliquité
        rotationAxis = Quaternion.Euler(rotationObliquity, 0f, 0f) * Vector3.up;
    }

    private void Start()
    {
        // Récupère le Soleil ici, Start() est appelé après tous les Awake()
        sunObj = GameObject.FindGameObjectWithTag("Sun");
        if (sunObj != null)
            sunScript = sunObj.GetComponent<StarControl>();

        if (sunScript == null)
        {
            Debug.LogError("[PlanetControl] No Sun (StarControl) found!");
            return;
        }

    }


    private void Update()
    {
        
        //transform.position *= (1f + hubbleFactor * Time.deltaTime);
        RotatePlanet();
    }

    private void FixedUpdate()
    {
        if (sunScript == null) return;
        // advance simulation time (use your timeScale)
        double dt = Time.fixedDeltaTime * timeScale;
        simulationTimeSeconds += dt;

        // Compute real position in meters from Kepler elements
        double a = keplerSemiMajorAxis;
        double e = keplerEccentricity;
        double iRad = keplerInclinationDeg * Mathf.Deg2Rad;
        double OmegaRad = keplerLongAscNodeDeg * Mathf.Deg2Rad;
        double omegaRad = keplerArgPeriapsisDeg * Mathf.Deg2Rad;
        double M0rad = keplerMeanAnomalyDeg * Mathf.Deg2Rad;

        // dt since epoch (seconds)
        double dtsinceEpoch = simulationTimeSeconds - keplerEpochSeconds;

        Vector3 posMeters = KeplerUtil.OrbitalElementsToPositionMeters(a, e, iRad, OmegaRad, omegaRad, M0rad, dtsinceEpoch, muCentral);

        // distance in meters (real distance)
        double realDistanceMeters = posMeters.magnitude;

        // convert real distance to logarithmic/compressed Unity distance using your ScaleDistance
        float scaledUnityDistance = ScaleDistance((float)realDistanceMeters);

        // compute direction from sun to body in meters, but we can use posMeters.normalized for direction
        Vector3 dir = posMeters.normalized;

        // place object in Unity using Sun position + scaled distance along direction
        Vector3 sunPos = sunObj.transform.position;
        transform.position = sunPos + dir * scaledUnityDistance;

        // Optionally also rotate the object to keep orbital frame aligned (not required)

    }
    #endregion

    #region Setup Methods
    //private void SetupComponents()
    //{
    //    planetRigidbody = GetComponent<Rigidbody>();
    //    if (planetRigidbody == null)
    //    {
    //        planetRigidbody = gameObject.AddComponent<Rigidbody>();
    //        Debug.Log($"[{nameof(PlanetControl)}] Added Rigidbody to {gameObject.name}.");
    //    }
    //    planetRigidbody.useGravity = false;
    //    planetRigidbody.isKinematic = true;
    //}


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

}
