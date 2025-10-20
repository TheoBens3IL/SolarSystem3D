using System;

public static class PhysicsConstants
{
    // Universal Constants
    public const double GRAVITATIONAL_CONSTANT = 6.67430e-11; // m³/kg/s²
    public const double SPEED_OF_LIGHT = 299792458.0; // m/s
    public const double ASTRONOMICAL_UNIT = 1.495978707e11; // m (1 AU)
    
    // Solar System Masses (kg)
    public const double SUN_MASS = 1.989e30;
    public const double MERCURY_MASS = 3.301e23;
    public const double VENUS_MASS = 4.867e24;
    public const double EARTH_MASS = 5.972e24;
    public const double MARS_MASS = 6.39e23;
    public const double JUPITER_MASS = 1.898e27;
    public const double SATURN_MASS = 5.683e26;
    public const double URANUS_MASS = 8.681e25;
    public const double NEPTUNE_MASS = 1.024e26;
    
    // Solar System Radii (m)
    public const double SUN_RADIUS = 6.96e8;
    public const double MERCURY_RADIUS = 2.439e6;
    public const double VENUS_RADIUS = 6.052e6;
    public const double EARTH_RADIUS = 6.371e6;
    public const double MARS_RADIUS = 3.390e6;
    public const double JUPITER_RADIUS = 6.991e7;
    public const double SATURN_RADIUS = 5.823e7;
    public const double URANUS_RADIUS = 2.536e7;
    public const double NEPTUNE_RADIUS = 2.462e7;
    
    // Time Constants
    public const double SECONDS_PER_DAY = 86400.0;
    public const double SECONDS_PER_HOUR = 3600.0;
    public const double DAYS_PER_YEAR = 365.25;
    
    // Conversion Factors
    public const double DEGREES_TO_RADIANS = Math.PI / 180.0;
    public const double RADIANS_TO_DEGREES = 180.0 / Math.PI;
    
    // Orbital Mechanics
    public static double CalculateOrbitalPeriod(double semiMajorAxis, double centralMass)
    {
        return 2.0 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / (GRAVITATIONAL_CONSTANT * centralMass));
    }
    
    public static double CalculateMeanMotion(double orbitalPeriod)
    {
        return 2.0 * Math.PI / orbitalPeriod;
    }
    
    public static double CalculateSemiMajorAxis(double orbitalPeriod, double centralMass)
    {
        return Math.Pow((GRAVITATIONAL_CONSTANT * centralMass * Math.Pow(orbitalPeriod, 2)) / (4.0 * Math.PI * Math.PI), 1.0 / 3.0);
    }
    
    // Kepler's Equation Solver (Newton-Raphson)
    public static double SolveKeplersEquation(double meanAnomaly, double eccentricity, double tolerance = 1e-10, int maxIterations = 100)
    {
        double E = meanAnomaly; // Initial guess
        
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
    
    // True Anomaly from Eccentric Anomaly
    public static double CalculateTrueAnomaly(double eccentricAnomaly, double eccentricity)
    {
        double cosE = Math.Cos(eccentricAnomaly);
        double sinE = Math.Sin(eccentricAnomaly);
        
        double cosNu = (cosE - eccentricity) / (1 - eccentricity * cosE);
        double sinNu = (Math.Sqrt(1 - eccentricity * eccentricity) * sinE) / (1 - eccentricity * cosE);
        
        return Math.Atan2(sinNu, cosNu);
    }
    
    // Orbital Position from Eccentric Anomaly
    public static (double x, double y) CalculateOrbitalPosition(double eccentricAnomaly, double semiMajorAxis, double eccentricity)
    {
        double cosE = Math.Cos(eccentricAnomaly);
        double sinE = Math.Sin(eccentricAnomaly);
        
        double x = semiMajorAxis * (cosE - eccentricity);
        double y = semiMajorAxis * Math.Sqrt(1 - eccentricity * eccentricity) * sinE;
        
        return (x, y);
    }
    
    // Orbital Velocity from Eccentric Anomaly
    public static (double vx, double vy) CalculateOrbitalVelocity(double eccentricAnomaly, double semiMajorAxis, double eccentricity, double meanMotion)
    {
        double cosE = Math.Cos(eccentricAnomaly);
        double sinE = Math.Sin(eccentricAnomaly);
        
        double factor = meanMotion * semiMajorAxis / (1 - eccentricity * cosE);
        
        double vx = -factor * sinE;
        double vy = factor * Math.Sqrt(1 - eccentricity * eccentricity) * cosE;
        
        return (vx, vy);
    }
    
    // Distance scaling for Unity
    public static double ScaleDistance(double realDistance, double scaleFactor)
    {
        return realDistance * scaleFactor;
    }
    
    // Size scaling for Unity
    public static double ScaleSize(double realSize, double scaleFactor)
    {
        return realSize * scaleFactor;
    }
    
    // Convert astronomical units to meters
    public static double AUToMeters(double au)
    {
        return au * ASTRONOMICAL_UNIT;
    }
    
    // Convert meters to astronomical units
    public static double MetersToAU(double meters)
    {
        return meters / ASTRONOMICAL_UNIT;
    }
    
    // Convert degrees to radians
    public static double ToRadians(double degrees)
    {
        return degrees * DEGREES_TO_RADIANS;
    }
    
    // Convert radians to degrees
    public static double ToDegrees(double radians)
    {
        return radians * RADIANS_TO_DEGREES;
    }
}
