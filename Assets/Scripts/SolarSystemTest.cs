using UnityEngine;

public class SolarSystemTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool runTestsOnStart = false;
    public bool verboseOutput = true;
    
    [Header("References")]
    public SolarSystemSetup setup;
    public SolarSystemSimulator simulator;
    public PlanetDataLoader dataLoader;

    void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        Debug.Log("=== Starting Solar System Tests ===");
        
        TestDataLoading();
        TestPlanetCreation();
        TestOrbitalMechanics();
        TestScaling();
        TestPerformance();
        
        Debug.Log("=== Solar System Tests Completed ===");
    }

    private void TestDataLoading()
    {
        Debug.Log("Testing data loading...");
        
        if (dataLoader == null)
        {
            dataLoader = FindObjectOfType<PlanetDataLoader>();
        }
        
        if (dataLoader == null)
        {
            Debug.LogError("Data loader not found!");
            return;
        }
        
        var planets = dataLoader.LoadPlanetData();
        
        if (planets.Count == 0)
        {
            Debug.LogError("No planet data loaded!");
            return;
        }
        
        Debug.Log($"✓ Successfully loaded {planets.Count} planets");
        
        // Test specific planet data
        var earth = dataLoader.GetPlanetData("Earth");
        if (earth != null)
        {
            Debug.Log($"✓ Earth data: Mass={earth.mass:E2} kg, Semi-major axis={earth.semiMajorAxis:E2} m");
        }
        else
        {
            Debug.LogError("✗ Earth data not found!");
        }
    }

    private void TestPlanetCreation()
    {
        Debug.Log("Testing planet creation...");
        
        if (simulator == null)
        {
            simulator = FindObjectOfType<SolarSystemSimulator>();
        }
        
        if (simulator == null)
        {
            Debug.LogError("Simulator not found!");
            return;
        }
        
        var planets = simulator.GetPlanets();
        
        if (planets.Count == 0)
        {
            Debug.LogError("No planets created!");
            return;
        }
        
        Debug.Log($"✓ Successfully created {planets.Count} planets");
        
        // Test planet properties
        foreach (var planet in planets)
        {
            if (planet.planetData == null)
            {
                Debug.LogError($"✗ Planet {planet.planetName} has no data!");
                continue;
            }
            
            if (verboseOutput)
            {
                Debug.Log($"✓ {planet.planetName}: Position={planet.transform.position}, Scale={planet.transform.localScale}");
            }
        }
    }

    private void TestOrbitalMechanics()
    {
        Debug.Log("Testing orbital mechanics...");
        
        if (simulator == null)
        {
            simulator = FindObjectOfType<SolarSystemSimulator>();
        }
        
        var planets = simulator.GetPlanets();
        
        foreach (var planet in planets)
        {
            if (planet.planetData == null) continue;
            
            // Test orbital period calculation
            double expectedPeriod = PhysicsConstants.CalculateOrbitalPeriod(
                planet.planetData.semiMajorAxis, 
                PhysicsConstants.SUN_MASS
            );
            
            double actualPeriod = planet.planetData.orbitalPeriod;
            double error = Math.Abs(expectedPeriod - actualPeriod) / expectedPeriod;
            
            if (error < 0.01) // 1% error tolerance
            {
                Debug.Log($"✓ {planet.planetName} orbital period calculation correct");
            }
            else
            {
                Debug.LogError($"✗ {planet.planetName} orbital period error: {error:P2}");
            }
        }
    }

    private void TestScaling()
    {
        Debug.Log("Testing scaling...");
        
        if (simulator == null)
        {
            simulator = FindObjectOfType<SolarSystemSimulator>();
        }
        
        var planets = simulator.GetPlanets();
        
        foreach (var planet in planets)
        {
            if (planet.planetData == null) continue;
            
            // Test distance scaling
            Vector3d orbitalPos = planet.GetOrbitalPosition();
            Vector3 scaledPos = (orbitalPos * simulator.distanceScale).ToVector3();
            
            if (scaledPos.magnitude > 0.1f && scaledPos.magnitude < 10000f)
            {
                Debug.Log($"✓ {planet.planetName} distance scaling reasonable: {scaledPos.magnitude:F2}");
            }
            else
            {
                Debug.LogWarning($"⚠ {planet.planetName} distance scaling may be off: {scaledPos.magnitude:F2}");
            }
            
            // Test size scaling
            float expectedSize = (float)(planet.planetData.diameter * simulator.sizeScale);
            float actualSize = planet.transform.localScale.x;
            
            if (Math.Abs(expectedSize - actualSize) < 0.01f)
            {
                Debug.Log($"✓ {planet.planetName} size scaling correct");
            }
            else
            {
                Debug.LogError($"✗ {planet.planetName} size scaling error: expected={expectedSize:F3}, actual={actualSize:F3}");
            }
        }
    }

    private void TestPerformance()
    {
        Debug.Log("Testing performance...");
        
        if (simulator == null)
        {
            simulator = FindObjectOfType<SolarSystemSimulator>();
        }
        
        var planets = simulator.GetPlanets();
        
        // Test update performance
        float startTime = Time.realtimeSinceStartup;
        
        for (int i = 0; i < 1000; i++)
        {
            foreach (var planet in planets)
            {
                if (planet.orbit != null)
                {
                    planet.orbit.UpdateOrbit(0.016); // 60 FPS
                }
            }
        }
        
        float endTime = Time.realtimeSinceStartup;
        float totalTime = endTime - startTime;
        
        Debug.Log($"✓ Performance test: {totalTime:F3}s for 1000 updates of {planets.Count} planets");
        
        if (totalTime < 1.0f)
        {
            Debug.Log("✓ Performance is good");
        }
        else
        {
            Debug.LogWarning("⚠ Performance may be slow");
        }
    }

    [ContextMenu("Test Data Loading Only")]
    public void TestDataLoadingOnly()
    {
        TestDataLoading();
    }

    [ContextMenu("Test Planet Creation Only")]
    public void TestPlanetCreationOnly()
    {
        TestPlanetCreation();
    }

    [ContextMenu("Test Orbital Mechanics Only")]
    public void TestOrbitalMechanicsOnly()
    {
        TestOrbitalMechanics();
    }

    [ContextMenu("Test Scaling Only")]
    public void TestScalingOnly()
    {
        TestScaling();
    }

    [ContextMenu("Test Performance Only")]
    public void TestPerformanceOnly()
    {
        TestPerformance();
    }
}
