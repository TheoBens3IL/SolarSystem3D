using UnityEngine;

/// <summary>
/// Central repository for physical constants used across the solar system simulation.
/// Keeps everything consistent and easy to maintain.
/// </summary>
public static class PhysicsConstants
{
    #region Universal Constants

    /// <summary>
    /// Gravitational constant (m³ / kg / s²)
    /// </summary>
    public const double G = 6.67430e-11;

    /// <summary>
    /// Astronomical Unit in meters (average Earth-Sun distance)
    /// </summary>
    public const double AU = 1.495978707e11;

    // --- Gravitational parameters (mu = G * M) ---
    public static readonly double MuSun = G * MassSun;

    /// <summary>
    /// Speed of light in meters per second
    /// </summary>
    public const double SpeedOfLight = 299_792_458.0;

    #endregion

    #region Solar System Constants

    /// <summary>
    /// Solar mass (kg)
    /// </summary>
    public const double MassSun = 1.98847e30;

    /// <summary>
    /// Solar radius (meters)
    /// </summary>
    public const double RadiusSun = 6.9634e8;

    #endregion

    #region Time Constants

    /// <summary>
    /// Seconds in one day
    /// </summary>
    public const double SecondsPerDay = 86400.0;

    /// <summary>
    /// Seconds in one year (Julian year)
    /// </summary>
    public const double SecondsPerYear = 3.15576e7;

    #endregion
}
