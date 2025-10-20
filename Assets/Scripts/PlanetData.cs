using System;
using UnityEngine;

[System.Serializable]
public class PlanetData
{
    public string name;
    public double mass; // kg
    public double diameter; // m
    public double semiMajorAxis; // m (a)
    public double eccentricity; // e
    public double inclination; // degrees (i)
    public double longitudeOfAscendingNode; // degrees (Ω)
    public double argumentOfPeriapsis; // degrees (ω)
    public double meanAnomalyAtEpoch; // degrees (M0)
    public double epoch; // epoch time
    public double distance; // distance scaling factor
    public double alpha; // scaling factor
    public double beta; // scaling factor
    public double gamma; // scaling factor
    public double rotationPeriod; // hours
    public double rotationSpeedMultiplier;
    public double rotationObliquity; // degrees

    // Calculated properties
    public double orbitalPeriod; // seconds
    public double meanMotion; // radians per second
    public double gravitationalParameter; // m³/s²

    public PlanetData(string[] csvRow)
    {
        if (csvRow.Length < 16)
        {
            Debug.LogError($"Insufficient data in CSV row for planet: {csvRow[0]}");
            return;
        }

        name = csvRow[0];
        mass = ParseDouble(csvRow[1]);
        diameter = ParseDouble(csvRow[2]);
        semiMajorAxis = ParseDouble(csvRow[3]);
        eccentricity = ParseDouble(csvRow[4]);
        inclination = ParseDouble(csvRow[5]);
        longitudeOfAscendingNode = ParseDouble(csvRow[6]);
        argumentOfPeriapsis = ParseDouble(csvRow[7]);
        meanAnomalyAtEpoch = ParseDouble(csvRow[8]);
        epoch = ParseDouble(csvRow[9]);
        distance = ParseDouble(csvRow[10]);
        alpha = ParseDouble(csvRow[11]);
        beta = ParseDouble(csvRow[12]);
        gamma = ParseDouble(csvRow[13]);
        rotationPeriod = ParseDouble(csvRow[14]);
        rotationSpeedMultiplier = ParseDouble(csvRow[15]);
        rotationObliquity = ParseDouble(csvRow[16]);

        // Calculate derived properties
        CalculateDerivedProperties();
    }

    private void CalculateDerivedProperties()
    {
        // Gravitational parameter μ = GM (where G is the gravitational constant)
        const double G = 6.67430e-11; // m³/kg/s²
        gravitationalParameter = G * mass;

        // Orbital period using Kepler's third law: T = 2π√(a³/μ)
        orbitalPeriod = 2.0 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / gravitationalParameter);

        // Mean motion n = 2π/T
        meanMotion = 2.0 * Math.PI / orbitalPeriod;
    }

    private double ParseDouble(string value)
    {
        if (double.TryParse(value, out double result))
            return result;
        
        Debug.LogWarning($"Failed to parse double value: {value}");
        return 0.0;
    }

    // Convert degrees to radians
    public static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    // Convert radians to degrees
    public static double RadiansToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}
