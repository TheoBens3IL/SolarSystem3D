using System.Collections.Generic;
using UnityEngine;

public class SolarSystemSimulator : MonoBehaviour
{
    [Header("Data Source")]
    public PlanetDataLoader dataLoader;
    
    [Header("Planet Prefabs")]
    public GameObject sunPrefab;
    public GameObject[] planetPrefabs;
    
    [Header("Scaling Settings")]
    [Range(1e-12f, 1e-6f)]
    public float distanceScale = 1e-9f;
    [Range(1e-6f, 1e-2f)]
    public float sizeScale = 1e-6f;
    
    [Header("Simulation Settings")]
    public float timeScale = 1.0f;
    public bool autoStart = true;
    public bool enableOrbits = true;
    public bool enableRotation = true;
    
    [Header("Sun Settings")]
    public float sunSize = 100f; // Unity units
    public Color sunColor = Color.yellow;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    private List<PlanetControl> planets = new List<PlanetControl>();
    private GameObject sun;
    private bool simulationStarted = false;

    void Start()
    {
        if (autoStart)
        {
            InitializeSolarSystem();
        }
    }

    void Update()
    {
        if (simulationStarted)
        {
            UpdateSimulation();
        }
    }

    public void InitializeSolarSystem()
    {
        if (dataLoader == null)
        {
            Debug.LogError("PlanetDataLoader not assigned!");
            return;
        }

        // Load planet data
        List<PlanetData> planetDataList = dataLoader.LoadPlanetData();
        if (planetDataList.Count == 0)
        {
            Debug.LogError("No planet data loaded!");
            return;
        }

        // Create sun
        CreateSun();

        // Create planets
        CreatePlanets(planetDataList);

        simulationStarted = true;
        Debug.Log($"Solar System initialized with {planets.Count} planets");
    }

    private void CreateSun()
    {
        if (sunPrefab != null)
        {
            sun = Instantiate(sunPrefab, Vector3.zero, Quaternion.identity);
            sun.name = "Sun";
            sun.transform.localScale = Vector3.one * sunSize;
            
            // Set sun color
            Renderer sunRenderer = sun.GetComponent<Renderer>();
            if (sunRenderer != null)
            {
                sunRenderer.material.color = sunColor;
            }
        }
        else
        {
            // Create a simple sun if no prefab
            GameObject sunObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sunObject.name = "Sun";
            sunObject.transform.position = Vector3.zero;
            sunObject.transform.localScale = Vector3.one * sunSize;
            sunObject.GetComponent<Renderer>().material.color = sunColor;
            sun = sunObject;
        }
    }

    private void CreatePlanets(List<PlanetData> planetDataList)
    {
        foreach (PlanetData data in planetDataList)
        {
            GameObject planetPrefab = GetPlanetPrefab(data.name);
            if (planetPrefab == null)
            {
                Debug.LogWarning($"No prefab found for planet: {data.name}");
                continue;
            }

            GameObject planet = Instantiate(planetPrefab);
            planet.name = data.name;

            // Add PlanetControl component
            PlanetControl planetControl = planet.GetComponent<PlanetControl>();
            if (planetControl == null)
            {
                planetControl = planet.AddComponent<PlanetControl>();
            }

            // Configure planet
            planetControl.planetName = data.name;
            planetControl.planetData = data;
            planetControl.distanceScale = distanceScale;
            planetControl.sizeScale = sizeScale;
            planetControl.timeScale = timeScale;
            planetControl.enableOrbit = enableOrbits;
            planetControl.enableRotation = enableRotation;
            planetControl.showDebugInfo = showDebugInfo;

            planets.Add(planetControl);

            if (showDebugInfo)
            {
                Debug.Log($"Created planet: {data.name}");
            }
        }
    }

    private GameObject GetPlanetPrefab(string planetName)
    {
        foreach (GameObject prefab in planetPrefabs)
        {
            if (prefab.name.Contains(planetName))
            {
                return prefab;
            }
        }
        return null;
    }

    private void UpdateSimulation()
    {
        // Update all planets
        foreach (PlanetControl planet in planets)
        {
            if (planet != null)
            {
                planet.SetTimeScale(timeScale);
                planet.SetDistanceScale(distanceScale);
                planet.SetSizeScale(sizeScale);
            }
        }
    }

    public void SetTimeScale(float newTimeScale)
    {
        timeScale = newTimeScale;
    }

    public void SetDistanceScale(float newDistanceScale)
    {
        distanceScale = newDistanceScale;
        
        foreach (PlanetControl planet in planets)
        {
            if (planet != null)
            {
                planet.SetDistanceScale(distanceScale);
            }
        }
    }

    public void SetSizeScale(float newSizeScale)
    {
        sizeScale = newSizeScale;
        
        foreach (PlanetControl planet in planets)
        {
            if (planet != null)
            {
                planet.SetSizeScale(sizeScale);
            }
        }
    }

    public void ToggleOrbits()
    {
        enableOrbits = !enableOrbits;
        
        foreach (PlanetControl planet in planets)
        {
            if (planet != null)
            {
                planet.enableOrbit = enableOrbits;
            }
        }
    }

    public void ToggleRotation()
    {
        enableRotation = !enableRotation;
        
        foreach (PlanetControl planet in planets)
        {
            if (planet != null)
            {
                planet.enableRotation = enableRotation;
            }
        }
    }

    public void ResetSimulation()
    {
        // Destroy all planets
        foreach (PlanetControl planet in planets)
        {
            if (planet != null)
            {
                DestroyImmediate(planet.gameObject);
            }
        }
        planets.Clear();

        // Destroy sun
        if (sun != null)
        {
            DestroyImmediate(sun);
        }

        simulationStarted = false;
        
        // Reinitialize
        InitializeSolarSystem();
    }

    public List<PlanetControl> GetPlanets()
    {
        return planets;
    }

    public PlanetControl GetPlanet(string planetName)
    {
        foreach (PlanetControl planet in planets)
        {
            if (planet.planetName.Equals(planetName, System.StringComparison.OrdinalIgnoreCase))
            {
                return planet;
            }
        }
        return null;
    }

    void OnGUI()
    {
        if (showDebugInfo && simulationStarted)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Solar System Controls", GUI.skin.box);
            
            GUILayout.Label($"Time Scale: {timeScale:F2}");
            timeScale = GUILayout.HorizontalSlider(timeScale, 0.1f, 10f);
            
            GUILayout.Label($"Distance Scale: {distanceScale:E2}");
            distanceScale = GUILayout.HorizontalSlider(distanceScale, 1e-12f, 1e-6f);
            
            GUILayout.Label($"Size Scale: {sizeScale:E2}");
            sizeScale = GUILayout.HorizontalSlider(sizeScale, 1e-6f, 1e-2f);
            
            if (GUILayout.Button("Toggle Orbits"))
                ToggleOrbits();
                
            if (GUILayout.Button("Toggle Rotation"))
                ToggleRotation();
                
            if (GUILayout.Button("Reset Simulation"))
                ResetSimulation();
                
            GUILayout.EndArea();
        }
    }
}
