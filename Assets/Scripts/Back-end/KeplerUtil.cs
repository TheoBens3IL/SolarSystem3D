using System;
using UnityEngine;

/// <summary>
/// Ensemble de fonctions pour calculer la position d'un corps à partir de ses éléments de Kepler.
/// Les distances sont exprimées en mètres, les angles en radians et le temps en secondes.
/// </summary>
public static class KeplerUtil
{
    /// <summary>
    /// Résout l'anomalie excentrique E à partir de l'anomalie moyenne M avec la méthode de Newton-Raphson.
    /// </summary>
    public static double SolveEccentricAnomaly(double M, double e, int maxIter = 60, double tol = 1e-12)
    {
        M = NormalizeAngleRad(M);
        double E = (e < 0.8) ? M : Math.PI;

        for (int i = 0; i < maxIter; i++)
        {
            double f = E - e * Math.Sin(E) - M;
            double fp = 1 - e * Math.Cos(E);
            double dE = f / fp;
            E -= dE;
            if (Math.Abs(dE) < tol) break;
        }

        return E;
    }

    /// <summary>
    /// Normalise un angle en radians dans l'intervalle [-π, π).
    /// </summary>
    private static double NormalizeAngleRad(double a)
    {
        double twoPi = 2.0 * Math.PI;
        a %= twoPi;
        if (a < -Math.PI) a += twoPi;
        if (a >= Math.PI) a -= twoPi;
        return a;
    }

    /// <summary>
    /// Calcule la position héliocentrique d'un corps à partir de ses éléments orbitaux.
    /// </summary>
    /// <param name="a">Demi-grand axe (m)</param>
    /// <param name="e">Excentricité</param>
    /// <param name="i">Inclinaison (rad)</param>
    /// <param name="Omega">Longitude du nœud ascendant (rad)</param>
    /// <param name="omega">Argument du périhélie (rad)</param>
    /// <param name="M0">Anomalie moyenne à l'époque t0 (rad)</param>
    /// <param name="dtSeconds">Temps écoulé depuis t0 (s)</param>
    /// <param name="mu">Constante gravitationnelle (G*M, m³/s²)</param>
    public static Vector3 OrbitalElementsToPositionMeters(
        double a, double e, double i, double Omega, double omega, double M0, double dtSeconds, double mu)
    {
        // Mouvement moyen (rad/s)
        double n = Math.Sqrt(mu / (a * a * a));

        // Anomalie moyenne à l'instant t
        double M = M0 + n * dtSeconds;

        // Anomalie excentrique
        double E = SolveEccentricAnomaly(M, e);

        // Anomalie vraie
        double cosE = Math.Cos(E);
        double sinE = Math.Sin(E);
        double sqrtOneMinusESq = Math.Sqrt(Math.Max(0.0, 1.0 - e * e));
        double v = Math.Atan2(sqrtOneMinusESq * sinE, cosE - e);

        // Distance au foyer
        double r = a * (1.0 - e * cosE);

        // Coordonnées dans le plan orbital
        double xOrb = r * Math.Cos(v);
        double yOrb = r * Math.Sin(v);

        // Rotations successives pour passer du plan orbital au plan de référence
        double cosw = Math.Cos(omega), sinw = Math.Sin(omega);
        double x1 = cosw * xOrb - sinw * yOrb;
        double y1 = sinw * xOrb + cosw * yOrb;

        double cosi = Math.Cos(i), sini = Math.Sin(i);
        double x2 = x1;
        double y2 = cosi * y1;
        double z2 = sini * y1;

        double cosO = Math.Cos(Omega), sinO = Math.Sin(Omega);
        double x = cosO * x2 - sinO * y2;
        double y = sinO * x2 + cosO * y2;
        double z = z2;

        // Retourne la position sous forme de Vector3 (mètres) selon la convention Y-up de Unity
        return new Vector3((float)x, (float)z, (float)y);
    }
}
