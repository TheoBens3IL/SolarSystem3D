using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetControl : MonoBehaviour
{
    [Header("Planet Data")]
    public string planetName;
    public PlanetData planetData;
    
    [Header("Scaling Settings")]
    [Range(1e-12f, 1e-6f)]
    public float distanceScale = 1e-9f;
    [Range(1e-6f, 1e-2f)]
    public float sizeScale = 1e-6f;
    
    [Header("Simulation Settings")]
    public float timeScale = 1.0f;
    public bool enableRotation = true;
    public bool enableOrbit = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    private KeplerianOrbit orbit;
    private Vector3d initialPosition;
    private Vector3d initialVelocity;
    private float rotationSpeed;
    private Vector3 rotationAxis;

    void Start()
    {
        InitializePlanet();
    }

    void Update()
    {
        if (enableOrbit && orbit != null)
        {
            UpdateOrbit();
        }
        
        if (enableRotation)
        {
            UpdateRotation();
        }
    }

    private void InitializePlanet()
    {
        if (planetData == null)
        {
            Debug.LogError($"Planet data not assigned for {planetName}");
            return;
        }

        // Initialize orbital mechanics
        orbit = new KeplerianOrbit(planetData);
        
        // Set initial position and velocity
        orbit.UpdateOrbit(0.0);
        initialPosition = orbit.GetPosition();
        initialVelocity = orbit.GetVelocity();
        
        // Scale position for Unity
        Vector3 scaledPosition = (initialPosition * distanceScale).ToVector3();
        transform.position = scaledPosition;
        
        // Scale planet size
        float scaledDiameter = (float)(planetData.diameter * sizeScale);
        transform.localScale = Vector3.one * scaledDiameter;
        
        // Setup rotation
        SetupRotation();
        
        if (showDebugInfo)
        {
            Debug.Log($"Initialized {planetName}: " +
                     $"Position: {scaledPosition}, " +
                     $"Scale: {scaledDiameter}, " +
                     $"Orbital Period: {planetData.orbitalPeriod / 86400:F2} days");
        }
    }

    private void UpdateOrbit()
    {
        if (orbit == null) return;

        // Update orbital position
        orbit.UpdateOrbit(Time.deltaTime * timeScale);
        Vector3d currentPosition = orbit.GetPosition();
        
        // Scale position for Unity
        Vector3 scaledPosition = (currentPosition * distanceScale).ToVector3();
        transform.position = scaledPosition;
    }

    private void UpdateRotation()
    {
        if (rotationSpeed > 0)
        {
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime * timeScale);
        }
    }

    private void SetupRotation()
    {
        if (planetData == null) return;

        // Calculate rotation speed in degrees per second
        // Convert rotation period from hours to seconds, then to degrees per second
        double rotationPeriodSeconds = planetData.rotationPeriod * 3600.0; // hours to seconds
        rotationSpeed = (float)(360.0 / rotationPeriodSeconds); // degrees per second
        
        // Apply rotation speed multiplier
        rotationSpeed *= (float)planetData.rotationSpeedMultiplier;
        
        // Set rotation axis (tilt based on obliquity)
        float obliquity = (float)planetData.rotationObliquity;
        rotationAxis = Quaternion.Euler(0, 0, obliquity) * Vector3.up;
        
        if (showDebugInfo)
        {
            Debug.Log($"{planetName} rotation: {rotationSpeed:F4} deg/s, " +
                     $"obliquity: {obliquity:F2}°, " +
                     $"axis: {rotationAxis}");
        }
    }

    public void SetTimeScale(float newTimeScale)
    {
        timeScale = newTimeScale;
    }

    public void SetDistanceScale(float newDistanceScale)
    {
        distanceScale = newDistanceScale;
        
        // Update current position with new scale
        if (orbit != null)
        {
            Vector3d currentPosition = orbit.GetPosition();
            Vector3 scaledPosition = (currentPosition * distanceScale).ToVector3();
            transform.position = scaledPosition;
        }
    }

    public void SetSizeScale(float newSizeScale)
    {
        sizeScale = newSizeScale;
        
        // Update planet size
        if (planetData != null)
        {
            float scaledDiameter = (float)(planetData.diameter * sizeScale);
            transform.localScale = Vector3.one * scaledDiameter;
        }
    }

    public Vector3d GetOrbitalPosition()
    {
        return orbit?.GetPosition() ?? Vector3d.zero;
    }

    public Vector3d GetOrbitalVelocity()
    {
        return orbit?.GetVelocity() ?? Vector3d.zero;
    }

    public double GetOrbitalPeriod()
    {
        return planetData?.orbitalPeriod ?? 0.0;
    }

    void OnDrawGizmos()
    {
        if (showDebugInfo && planetData != null)
        {
            // Draw orbital path preview
            DrawOrbitalPath();
        }
    }

    private void DrawOrbitalPath()
    {
        if (orbit == null) return;

        Gizmos.color = Color.yellow;
        int segments = 100;
        float angleStep = 360f / segments;
        
        Vector3d previousPosition = Vector3d.zero;
        bool firstPoint = true;

        for (int i = 0; i <= segments; i++)
        {
            // Calculate position for this angle
            double angle = i * angleStep * Mathf.Deg2Rad;
            double eccentricAnomaly = SolveKeplersEquationForAngle(angle);
            Vector3d orbitalPos = CalculateOrbitalPositionForAngle(eccentricAnomaly);
            Vector3d worldPos = TransformTo3DSpace(orbitalPos);
            Vector3 scaledPos = (worldPos * distanceScale).ToVector3();

            if (!firstPoint)
            {
                Gizmos.DrawLine(previousPosition.ToVector3(), scaledPos);
            }
            
            previousPosition = worldPos;
            firstPoint = false;
        }
    }

    private double SolveKeplersEquationForAngle(double angle)
    {
        // Simplified for circular orbits - in reality this should use the full Kepler equation
        return angle;
    }

    private Vector3d CalculateOrbitalPositionForAngle(double eccentricAnomaly)
    {
        double a = planetData.semiMajorAxis;
        double e = planetData.eccentricity;
        
        double cosE = Math.Cos(eccentricAnomaly);
        double sinE = Math.Sin(eccentricAnomaly);
        
        double x = a * (cosE - e);
        double y = a * Math.Sqrt(1 - e * e) * sinE;
        
        return new Vector3d(x, y, 0);
    }

    private Vector3d TransformTo3DSpace(Vector3d orbitalVector)
    {
        // Same transformation as in KeplerianOrbit
        double i = PlanetData.DegreesToRadians(planetData.inclination);
        double omega = PlanetData.DegreesToRadians(planetData.longitudeOfAscendingNode);
        double w = PlanetData.DegreesToRadians(planetData.argumentOfPeriapsis);

        double cosOmega = Math.Cos(omega);
        double sinOmega = Math.Sin(omega);
        double cosI = Math.Cos(i);
        double sinI = Math.Sin(i);
        double cosW = Math.Cos(w);
        double sinW = Math.Sin(w);

        double[,] R = new double[3, 3];
        
        R[0, 0] = cosOmega * cosW - sinOmega * cosI * sinW;
        R[0, 1] = -cosOmega * sinW - sinOmega * cosI * cosW;
        R[0, 2] = sinOmega * sinI;
        
        R[1, 0] = sinOmega * cosW + cosOmega * cosI * sinW;
        R[1, 1] = -sinOmega * sinW + cosOmega * cosI * cosW;
        R[1, 2] = -cosOmega * sinI;
        
        R[2, 0] = sinI * sinW;
        R[2, 1] = sinI * cosW;
        R[2, 2] = cosI;

        double x = R[0, 0] * orbitalVector.x + R[0, 1] * orbitalVector.y + R[0, 2] * orbitalVector.z;
        double y = R[1, 0] * orbitalVector.x + R[1, 1] * orbitalVector.y + R[1, 2] * orbitalVector.z;
        double z = R[2, 0] * orbitalVector.x + R[2, 1] * orbitalVector.y + R[2, 2] * orbitalVector.z;

        return new Vector3d(x, y, z);
    }
}
