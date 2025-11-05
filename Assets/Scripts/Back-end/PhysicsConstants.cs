using UnityEngine;

/// <summary>
/// Contient l'ensemble des constantes physiques utilisées dans la simulation du système solaire.
/// Permet de garantir la cohérence des calculs à travers tous les scripts.
/// </summary>
public static class PhysicsConstants
{
    #region Constantes universelles

    /// <summary>
    /// Constante gravitationnelle (m³ / kg / s²)
    /// </summary>
    public const double G = 6.67430e-11;

    /// <summary>
    /// Unité astronomique en mètres (distance moyenne Terre–Soleil)
    /// </summary>
    public const double AU = 1.495978707e11;

    /// <summary>
    /// Paramètre gravitationnel du Soleil (µ = G × masse du Soleil)
    /// </summary>
    public static readonly double MuSun = G * MassSun;

    /// <summary>
    /// Vitesse de la lumière (m/s)
    /// </summary>
    public const double SpeedOfLight = 299_792_458.0;

    #endregion

    #region Constantes solaires

    /// <summary>
    /// Masse du Soleil (kg)
    /// </summary>
    public const double MassSun = 1.98847e30;

    /// <summary>
    /// Rayon du Soleil (m)
    /// </summary>
    public const double RadiusSun = 6.9634e8;

    #endregion

    #region Constantes temporelles

    /// <summary>
    /// Nombre de secondes dans une journée
    /// </summary>
    public const double SecondsPerDay = 86400.0;

    /// <summary>
    /// Nombre de secondes dans une année julienne
    /// </summary>
    public const double SecondsPerYear = 3.15576e7;

    #endregion
}
