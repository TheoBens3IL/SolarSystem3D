using System.Collections.Generic;
using UnityEngine;

public class OrbitManager : MonoBehaviour
{
    [Header("Orbit Settings")]
    public bool showAllOrbits = true;
    public Color[] orbitColors = { Color.white, Color.blue, Color.green, Color.red, Color.yellow, Color.cyan, Color.magenta, Color.gray };
    public float orbitLineWidth = 0.1f;
    public int orbitSegments = 100;
    public Material orbitMaterial;
    
    [Header("Planet References")]
    public SolarSystemSimulator solarSystemSimulator;
    
    [Header("Debug")]
    public bool debugMode = false;

    private List<OrbitDrawer> orbitDrawers = new List<OrbitDrawer>();

    void Start()
    {
        if (solarSystemSimulator == null)
        {
            solarSystemSimulator = FindObjectOfType<SolarSystemSimulator>();
        }
        
        if (solarSystemSimulator != null)
        {
            // Wait for planets to be created
            Invoke(nameof(CreateOrbitDrawers), 1f);
        }
    }

    void Update()
    {
        if (debugMode)
        {
            UpdateOrbitVisibility();
        }
    }

    private void CreateOrbitDrawers()
    {
        if (solarSystemSimulator == null) return;

        List<PlanetControl> planets = solarSystemSimulator.GetPlanets();
        
        for (int i = 0; i < planets.Count; i++)
        {
            PlanetControl planet = planets[i];
            if (planet == null) continue;

            // Create orbit drawer object
            GameObject orbitObject = new GameObject($"{planet.planetName}_Orbit");
            orbitObject.transform.SetParent(transform);
            
            // Add OrbitDrawer component
            OrbitDrawer orbitDrawer = orbitObject.AddComponent<OrbitDrawer>();
            orbitDrawer.planetControl = planet;
            orbitDrawer.orbitSegments = orbitSegments;
            orbitDrawer.lineWidth = orbitLineWidth;
            orbitDrawer.orbitMaterial = orbitMaterial;
            orbitDrawer.showOrbit = showAllOrbits;
            
            // Set orbit color
            Color orbitColor = i < orbitColors.Length ? orbitColors[i] : Color.white;
            orbitDrawer.SetOrbitColor(orbitColor);
            
            orbitDrawers.Add(orbitDrawer);
            
            if (debugMode)
            {
                Debug.Log($"Created orbit drawer for {planet.planetName}");
            }
        }
    }

    public void ToggleAllOrbits()
    {
        showAllOrbits = !showAllOrbits;
        
        foreach (OrbitDrawer drawer in orbitDrawers)
        {
            if (drawer != null)
            {
                drawer.showOrbit = showAllOrbits;
                drawer.ToggleOrbit();
            }
        }
    }

    public void SetOrbitVisibility(bool visible)
    {
        showAllOrbits = visible;
        
        foreach (OrbitDrawer drawer in orbitDrawers)
        {
            if (drawer != null)
            {
                drawer.showOrbit = visible;
                drawer.ToggleOrbit();
            }
        }
    }

    public void SetOrbitLineWidth(float width)
    {
        orbitLineWidth = width;
        
        foreach (OrbitDrawer drawer in orbitDrawers)
        {
            if (drawer != null)
            {
                drawer.SetLineWidth(width);
            }
        }
    }

    public void SetOrbitSegments(int segments)
    {
        orbitSegments = segments;
        
        foreach (OrbitDrawer drawer in orbitDrawers)
        {
            if (drawer != null)
            {
                drawer.SetOrbitSegments(segments);
            }
        }
    }

    public void RecalculateAllOrbits()
    {
        foreach (OrbitDrawer drawer in orbitDrawers)
        {
            if (drawer != null)
            {
                drawer.CalculateOrbit();
            }
        }
    }

    private void UpdateOrbitVisibility()
    {
        // This can be used for dynamic orbit visibility based on camera distance, etc.
        // For now, just ensure all orbits are visible if showAllOrbits is true
    }

    public void ClearAllOrbits()
    {
        foreach (OrbitDrawer drawer in orbitDrawers)
        {
            if (drawer != null)
            {
                DestroyImmediate(drawer.gameObject);
            }
        }
        orbitDrawers.Clear();
    }

    public void RefreshOrbits()
    {
        ClearAllOrbits();
        CreateOrbitDrawers();
    }

    void OnGUI()
    {
        if (debugMode)
        {
            GUILayout.BeginArea(new Rect(10, 220, 300, 150));
            GUILayout.Label("Orbit Controls", GUI.skin.box);
            
            if (GUILayout.Button("Toggle All Orbits"))
                ToggleAllOrbits();
                
            if (GUILayout.Button("Recalculate Orbits"))
                RecalculateAllOrbits();
                
            if (GUILayout.Button("Refresh Orbits"))
                RefreshOrbits();
                
            GUILayout.Label($"Orbit Segments: {orbitSegments}");
            orbitSegments = (int)GUILayout.HorizontalSlider(orbitSegments, 20, 200);
            
            GUILayout.Label($"Line Width: {orbitLineWidth:F2}");
            orbitLineWidth = GUILayout.HorizontalSlider(orbitLineWidth, 0.01f, 1f);
                
            GUILayout.EndArea();
        }
    }
}
