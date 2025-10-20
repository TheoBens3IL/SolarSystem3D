using UnityEngine;

[System.Serializable]
public class SolarSystemConfig
{
    [Header("Scaling Settings")]
    [Range(1e-12f, 1e-6f)]
    public float distanceScale = 1e-9f;
    [Range(1e-6f, 1e-2f)]
    public float sizeScale = 1e-6f;
    
    [Header("Simulation Settings")]
    public float timeScale = 1.0f;
    public bool enableOrbits = true;
    public bool enableRotation = true;
    public bool autoStart = true;
    
    [Header("Sun Settings")]
    public float sunSize = 100f;
    public Color sunColor = Color.yellow;
    
    [Header("Orbit Settings")]
    public bool showOrbits = true;
    public float orbitLineWidth = 0.1f;
    public int orbitSegments = 100;
    public Color[] orbitColors = { Color.white, Color.blue, Color.green, Color.red, Color.yellow, Color.cyan, Color.magenta, Color.gray };
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    public bool debugMode = false;
}

public class SolarSystemSetup : MonoBehaviour
{
    [Header("Configuration")]
    public SolarSystemConfig config = new SolarSystemConfig();
    
    [Header("Required Components")]
    public PlanetDataLoader dataLoader;
    public SolarSystemSimulator simulator;
    public OrbitManager orbitManager;
    
    [Header("Planet Prefabs")]
    public GameObject sunPrefab;
    public GameObject[] planetPrefabs;

    void Start()
    {
        SetupSolarSystem();
    }

    [ContextMenu("Setup Solar System")]
    public void SetupSolarSystem()
    {
        // Setup data loader
        if (dataLoader == null)
        {
            dataLoader = FindObjectOfType<PlanetDataLoader>();
            if (dataLoader == null)
            {
                GameObject dataLoaderObj = new GameObject("PlanetDataLoader");
                dataLoader = dataLoaderObj.AddComponent<PlanetDataLoader>();
                
                // Try to find the CSV file
                TextAsset csvFile = Resources.Load<TextAsset>("PlanetsData");
                if (csvFile != null)
                {
                    dataLoader.csvFile = csvFile;
                }
                else
                {
                    Debug.LogError("PlanetsData.csv not found in Resources folder!");
                }
            }
        }

        // Setup simulator
        if (simulator == null)
        {
            simulator = FindObjectOfType<SolarSystemSimulator>();
            if (simulator == null)
            {
                GameObject simulatorObj = new GameObject("SolarSystemSimulator");
                simulator = simulatorObj.AddComponent<SolarSystemSimulator>();
            }
        }

        // Configure simulator
        simulator.dataLoader = dataLoader;
        simulator.sunPrefab = sunPrefab;
        simulator.planetPrefabs = planetPrefabs;
        simulator.distanceScale = config.distanceScale;
        simulator.sizeScale = config.sizeScale;
        simulator.timeScale = config.timeScale;
        simulator.enableOrbits = config.enableOrbits;
        simulator.enableRotation = config.enableRotation;
        simulator.autoStart = config.autoStart;
        simulator.sunSize = config.sunSize;
        simulator.sunColor = config.sunColor;
        simulator.showDebugInfo = config.showDebugInfo;

        // Setup orbit manager
        if (orbitManager == null)
        {
            orbitManager = FindObjectOfType<OrbitManager>();
            if (orbitManager == null)
            {
                GameObject orbitManagerObj = new GameObject("OrbitManager");
                orbitManager = orbitManagerObj.AddComponent<OrbitManager>();
            }
        }

        // Configure orbit manager
        orbitManager.solarSystemSimulator = simulator;
        orbitManager.showAllOrbits = config.showOrbits;
        orbitManager.orbitLineWidth = config.orbitLineWidth;
        orbitManager.orbitSegments = config.orbitSegments;
        orbitManager.orbitColors = config.orbitColors;
        orbitManager.debugMode = config.debugMode;

        Debug.Log("Solar System setup completed!");
    }

    [ContextMenu("Reset Solar System")]
    public void ResetSolarSystem()
    {
        if (simulator != null)
        {
            simulator.ResetSimulation();
        }
        
        if (orbitManager != null)
        {
            orbitManager.ClearAllOrbits();
        }
    }

    [ContextMenu("Apply Configuration")]
    public void ApplyConfiguration()
    {
        if (simulator != null)
        {
            simulator.SetDistanceScale(config.distanceScale);
            simulator.SetSizeScale(config.sizeScale);
            simulator.SetTimeScale(config.timeScale);
        }
        
        if (orbitManager != null)
        {
            orbitManager.SetOrbitLineWidth(config.orbitLineWidth);
            orbitManager.SetOrbitSegments(config.orbitSegments);
            orbitManager.SetOrbitVisibility(config.showOrbits);
        }
    }

    void OnValidate()
    {
        // Apply configuration when values change in inspector
        if (Application.isPlaying)
        {
            ApplyConfiguration();
        }
    }
}
