using UnityEngine;

/// <summary>
/// Singleton pour gérer le temps global de la simulation.
/// Toutes les planètes et systèmes s'appuient sur ce temps pour leurs calculs.
/// </summary>
public class SimulationClock : MonoBehaviour
{
    public static SimulationClock Instance { get; private set; }

    [Tooltip("Facteur d'accélération du temps de simulation (1 = temps réel).")]
    public float TimeScale = 100000f;

    // Temps total de la simulation en secondes
    public double SimulationTimeSeconds { get; private set; } = 0.0;

    private void Awake()
    {
        // Assure qu'il n'y a qu'une seule instance
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
        // Avance le temps en fonction de Time.deltaTime et du facteur TimeScale
        SimulationTimeSeconds += Time.deltaTime * TimeScale;
    }

    // Fournit le temps de simulation en jours
    public double SimulationTimeDays => SimulationTimeSeconds / 86400.0;
}
