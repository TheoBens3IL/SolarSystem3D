using UnityEngine;

public static class VisualScale
{
    // Default scaling parameters
    public static float BetaRadius = 0.1f;
    public static float GammaRadius = 0.5f;

    /// <summary>
    /// Scale a real radius (in meters) to a Unity-friendly visual scale.
    /// </summary>
    public static float ScaleRadius(float realRadius)
    {
        return BetaRadius * Mathf.Pow(realRadius, GammaRadius);
    }
}
