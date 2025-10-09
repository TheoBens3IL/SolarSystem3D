using System;
using UnityEngine;

/// <summary>
/// Utilities to compute position from Keplerian elements (double precision).
/// All distances are in meters, angles in radians, time in seconds.
/// </summary>
public static class KeplerUtil
{
    // Solve M = E - e*sin(E) with Newton-Raphson (E in radians)
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

    // Normalize angle to [-pi, pi)
    private static double NormalizeAngleRad(double a)
    {
        double twoPi = 2.0 * Math.PI;
        a = a % twoPi;
        if (a < -Math.PI) a += twoPi;
        if (a >= Math.PI) a -= twoPi;
        return a;
    }

    /// <summary>
    /// Compute heliocentric position vector in meters from Keplerian elements at time t (seconds).
    /// a = semi-major axis (m), e = eccentricity, i = inclination (rad),
    /// Omega = longitude of ascending node (rad), omega = argument of periapsis (rad),
    /// M0 = mean anomaly at epoch t0 (rad), t - t0 expressed in seconds.
    /// mu = G*(M_central) in SI (m^3/s^2)
    /// </summary>
    public static Vector3 OrbitalElementsToPositionMeters(
        double a, double e, double i, double Omega, double omega, double M0, double dtSeconds, double mu)
    {
        // mean motion n (rad/s)
        double n = Math.Sqrt(mu / (a * a * a));
        // current mean anomaly
        double M = M0 + n * dtSeconds;
        // solve eccentric anomaly
        double E = SolveEccentricAnomaly(M, e);
        // true anomaly
        double cosE = Math.Cos(E);
        double sinE = Math.Sin(E);
        double sqrtOneMinusESq = Math.Sqrt(Math.Max(0.0, 1.0 - e * e));
        double v = Math.Atan2(sqrtOneMinusESq * sinE, cosE - e); // true anomaly

        // distance r = a*(1 - e cos E)
        double r = a * (1.0 - e * cosE);

        // coordinates in orbital plane (x_orb, y_orb)
        double xOrb = r * Math.Cos(v);
        double yOrb = r * Math.Sin(v);
        double zOrb = 0.0;

        // Rotate from orbital plane to ecliptic: Rz(Omega) * Rx(i) * Rz(omega)
        // First rotate by omega around z:
        double cosw = Math.Cos(omega), sinw = Math.Sin(omega);
        double x1 = cosw * xOrb - sinw * yOrb;
        double y1 = sinw * xOrb + cosw * yOrb;
        double z1 = zOrb;

        // Rotate by i around x:
        double cosi = Math.Cos(i), sini = Math.Sin(i);
        double x2 = x1;
        double y2 = cosi * y1 - sini * z1;
        double z2 = sini * y1 + cosi * z1;

        // Rotate by Omega around z:
        double cosO = Math.Cos(Omega), sinO = Math.Sin(Omega);
        double x = cosO * x2 - sinO * y2;
        double y = sinO * x2 + cosO * y2;
        double z = z2;

        // Return as Unity Vector3 (meters)
        return new Vector3((float)x, (float)z, (float)y); // Y-up mapping same as earlier convention
    }
}
