using System;
using UnityEngine;

public class KeplerianOrbit
{
    private PlanetData planetData;
    private double currentTime;
    private double currentMeanAnomaly;
    private Vector3d currentPosition;
    private Vector3d currentVelocity;

    // Constants
    private const double G = 6.67430e-11; // Gravitational constant
    private const double SUN_MASS = 1.989e30; // kg

    public KeplerianOrbit(PlanetData data)
    {
        planetData = data;
        currentTime = 0.0;
        currentMeanAnomaly = PlanetData.DegreesToRadians(planetData.meanAnomalyAtEpoch);
    }

    public void UpdateOrbit(double deltaTime)
    {
        currentTime += deltaTime;
        
        // Calculate current mean anomaly
        double meanAnomaly = currentMeanAnomaly + planetData.meanMotion * deltaTime;
        currentMeanAnomaly = meanAnomaly;

        // Solve Kepler's equation to get eccentric anomaly
        double eccentricAnomaly = SolveKeplersEquation(meanAnomaly, planetData.eccentricity);

        // Calculate true anomaly
        double trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly, planetData.eccentricity);

        // Calculate position and velocity in orbital plane
        Vector3d orbitalPosition = CalculateOrbitalPosition(eccentricAnomaly);
        Vector3d orbitalVelocity = CalculateOrbitalVelocity(eccentricAnomaly);

        // Transform to 3D space using orbital elements
        currentPosition = TransformTo3DSpace(orbitalPosition);
        currentVelocity = TransformTo3DSpace(orbitalVelocity);
    }

    private double SolveKeplersEquation(double meanAnomaly, double eccentricity)
    {
        // Newton-Raphson method to solve E = M + e*sin(E)
        double E = meanAnomaly; // Initial guess
        double tolerance = 1e-10;
        int maxIterations = 100;

        for (int i = 0; i < maxIterations; i++)
        {
            double f = E - eccentricity * Math.Sin(E) - meanAnomaly;
            double fPrime = 1 - eccentricity * Math.Cos(E);
            
            if (Math.Abs(f) < tolerance)
                break;
                
            E = E - f / fPrime;
        }

        return E;
    }

    private double CalculateTrueAnomaly(double eccentricAnomaly, double eccentricity)
    {
        double cosE = Math.Cos(eccentricAnomaly);
        double sinE = Math.Sin(eccentricAnomaly);
        
        double cosNu = (cosE - eccentricity) / (1 - eccentricity * cosE);
        double sinNu = (Math.Sqrt(1 - eccentricity * eccentricity) * sinE) / (1 - eccentricity * cosE);
        
        return Math.Atan2(sinNu, cosNu);
    }

    private Vector3d CalculateOrbitalPosition(double eccentricAnomaly)
    {
        double a = planetData.semiMajorAxis;
        double e = planetData.eccentricity;
        
        double cosE = Math.Cos(eccentricAnomaly);
        double sinE = Math.Sin(eccentricAnomaly);
        
        double x = a * (cosE - e);
        double y = a * Math.Sqrt(1 - e * e) * sinE;
        
        return new Vector3d(x, y, 0);
    }

    private Vector3d CalculateOrbitalVelocity(double eccentricAnomaly)
    {
        double a = planetData.semiMajorAxis;
        double e = planetData.eccentricity;
        double n = planetData.meanMotion;
        
        double cosE = Math.Cos(eccentricAnomaly);
        double sinE = Math.Sin(eccentricAnomaly);
        
        double factor = n * a / (1 - e * cosE);
        
        double vx = -factor * sinE;
        double vy = factor * Math.Sqrt(1 - e * e) * cosE;
        
        return new Vector3d(vx, vy, 0);
    }

    private Vector3d TransformTo3DSpace(Vector3d orbitalVector)
    {
        // Convert orbital elements to rotation matrix
        double i = PlanetData.DegreesToRadians(planetData.inclination);
        double omega = PlanetData.DegreesToRadians(planetData.longitudeOfAscendingNode);
        double w = PlanetData.DegreesToRadians(planetData.argumentOfPeriapsis);

        // Rotation matrices
        double cosOmega = Math.Cos(omega);
        double sinOmega = Math.Sin(omega);
        double cosI = Math.Cos(i);
        double sinI = Math.Sin(i);
        double cosW = Math.Cos(w);
        double sinW = Math.Sin(w);

        // Combined rotation matrix: R = Rz(Ω) * Rx(i) * Rz(ω)
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

        // Apply rotation
        double x = R[0, 0] * orbitalVector.x + R[0, 1] * orbitalVector.y + R[0, 2] * orbitalVector.z;
        double y = R[1, 0] * orbitalVector.x + R[1, 1] * orbitalVector.y + R[1, 2] * orbitalVector.z;
        double z = R[2, 0] * orbitalVector.x + R[2, 1] * orbitalVector.y + R[2, 2] * orbitalVector.z;

        return new Vector3d(x, y, z);
    }

    public Vector3d GetPosition() => currentPosition;
    public Vector3d GetVelocity() => currentVelocity;
    public double GetCurrentTime() => currentTime;
}

// Custom Vector3d struct for double precision
[System.Serializable]
public struct Vector3d
{
    public double x, y, z;

    public Vector3d(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3d(Vector3 v)
    {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public static Vector3d operator +(Vector3d a, Vector3d b)
    {
        return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3d operator -(Vector3d a, Vector3d b)
    {
        return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3d operator *(Vector3d a, double scalar)
    {
        return new Vector3d(a.x * scalar, a.y * scalar, a.z * scalar);
    }

    public static Vector3d operator *(double scalar, Vector3d a)
    {
        return new Vector3d(a.x * scalar, a.y * scalar, a.z * scalar);
    }

    public double magnitude => Math.Sqrt(x * x + y * y + z * z);

    public Vector3d normalized
    {
        get
        {
            double mag = magnitude;
            if (mag < 1e-10) return new Vector3d(0, 0, 0);
            return new Vector3d(x / mag, y / mag, z / mag);
        }
    }
}
