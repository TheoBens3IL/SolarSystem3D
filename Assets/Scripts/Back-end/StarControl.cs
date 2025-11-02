using UnityEngine;

#region Documentation
/// <summary>
/// Gère l’étoile centrale du système solaire (ex: le Soleil).
/// Contient les paramètres physiques de l’étoile et fournit 
/// les données gravitationnelles aux planètes et corps orbitants.
/// </summary>
#endregion
[DisallowMultipleComponent]
public class StarControl : MonoBehaviour
{
    #region Serialized Fields

    [Header("Star Settings")]
    [Tooltip("Masse de l’étoile en kilogrammes.")]
    [SerializeField] private double starMass = PhysicsConstants.MassSun;

    [Tooltip("Diamètre physique réel de l’étoile (en mètres).")]
    [SerializeField] private float starDiameter = 1.3927e9f; // Soleil ~ 1 392 700 km

    [Header("Visual Settings")]
    [Tooltip("Multiplicateur pour ajuster la taille visuelle de l’étoile (1 = réaliste).")]
    [SerializeField, Range(0.1f, 100f)] private float visualScaleMultiplier = 3f;

    [Tooltip("Active le redimensionnement automatique de l’étoile.")]
    [SerializeField] private bool autoScale = true;

    [Header("Debug")]
    [Tooltip("Affiche les informations de l’étoile dans la console.")]
    [SerializeField] private bool debugInfo = false;

    #endregion

    #region Private Fields

    private double gravitationalParameter; // μ = G * M

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Calcule μ une fois au démarrage
        gravitationalParameter = PhysicsConstants.G * starMass;

        if (autoScale)
            ApplyVisualScale();

        if (debugInfo)
            Debug.Log($"[StarControl] Étoile initialisée : masse = {starMass:E2} kg, μ = {gravitationalParameter:E2}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Met à jour la taille visuelle instantanément dans l’éditeur si autoScale est activé
        if (autoScale && Application.isEditor)
            ApplyVisualScale();
    }
#endif

    #endregion

    #region Private Methods

    /// <summary>
    /// Met à l’échelle la représentation visuelle de l’étoile
    /// en fonction de son diamètre et du multiplicateur visuel.
    /// </summary>
    private void ApplyVisualScale()
    {
        float scaledDiameter = VisualScale.ScaleRadius(starDiameter) * visualScaleMultiplier;
        transform.localScale = Vector3.one * scaledDiameter;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Retourne le paramètre gravitationnel μ = G * M utilisé dans les calculs de Kepler.
    /// </summary>
    public double GetMu() => gravitationalParameter;

    /// <summary>
    /// Retourne la masse actuelle de l’étoile en kilogrammes.
    /// </summary>
    public double GetMass() => starMass;

    /// <summary>
    /// Met à jour manuellement la masse de l’étoile et recalcul μ.
    /// </summary>
    public void SetMass(double newMass)
    {
        starMass = newMass;
        gravitationalParameter = PhysicsConstants.G * starMass;
    }

    /// <summary>
    /// Force le recalcul de l’échelle visuelle (appel manuel possible).
    /// </summary>
    public void RefreshScale()
    {
        ApplyVisualScale();
    }

    #endregion
}
