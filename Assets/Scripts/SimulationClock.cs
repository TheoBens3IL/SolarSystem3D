using UnityEngine;

/// <summary>
/// Singleton global pour gérer le temps de simulation.
/// Toutes les planètes et systèmes utilisent ce temps.
/// </summary>
public class SimulationClock : MonoBehaviour
{
    public static SimulationClock Instance { get; private set; }

    [Tooltip("Vitesse de la simulation (1 = temps réel).")]
    public float TimeScale = 100000f;

    // Temps total de la simulation en secondes
    public double SimulationTimeSeconds { get; private set; } = 0.0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        // avance le temps en fonction de Time.deltaTime et TimeScale
        SimulationTimeSeconds += Time.deltaTime * TimeScale;
    }

    // Optionnel : pour obtenir le temps en jours
    public double SimulationTimeDays => SimulationTimeSeconds / 86400.0;
}
