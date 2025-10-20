using UnityEngine;

public class SolarSystemUI : MonoBehaviour
{
    [Header("References")]
    public SolarSystemSimulator simulator;
    public OrbitManager orbitManager;
    
    [Header("UI Settings")]
    public bool showUI = true;
    public KeyCode toggleUIKey = KeyCode.Tab;
    public KeyCode resetKey = KeyCode.R;
    public KeyCode toggleOrbitsKey = KeyCode.O;
    public KeyCode toggleRotationKey = KeyCode.T;
    
    [Header("Speed Controls")]
    public KeyCode speedUpKey = KeyCode.Plus;
    public KeyCode speedDownKey = KeyCode.Minus;
    public float speedStep = 0.5f;
    
    [Header("Scale Controls")]
    public KeyCode scaleUpKey = KeyCode.Equals;
    public KeyCode scaleDownKey = KeyCode.Minus;
    public float scaleStep = 1.1f;

    private bool uiVisible = true;

    void Start()
    {
        if (simulator == null)
            simulator = FindObjectOfType<SolarSystemSimulator>();
        if (orbitManager == null)
            orbitManager = FindObjectOfType<OrbitManager>();
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Toggle UI
        if (Input.GetKeyDown(toggleUIKey))
        {
            uiVisible = !uiVisible;
        }

        // Reset simulation
        if (Input.GetKeyDown(resetKey))
        {
            if (simulator != null)
            {
                simulator.ResetSimulation();
            }
        }

        // Toggle orbits
        if (Input.GetKeyDown(toggleOrbitsKey))
        {
            if (orbitManager != null)
            {
                orbitManager.ToggleAllOrbits();
            }
        }

        // Toggle rotation
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (simulator != null)
            {
                simulator.ToggleRotation();
            }
        }

        // Speed controls
        if (simulator != null)
        {
            if (Input.GetKeyDown(speedUpKey))
            {
                simulator.SetTimeScale(simulator.timeScale + speedStep);
            }
            if (Input.GetKeyDown(speedDownKey))
            {
                simulator.SetTimeScale(Mathf.Max(0.1f, simulator.timeScale - speedStep));
            }
        }
    }

    void OnGUI()
    {
        if (!showUI || !uiVisible) return;

        // Main control panel
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("Solar System Controls", GUI.skin.box);
        GUILayout.Space(10);

        // Simulation controls
        if (simulator != null)
        {
            GUILayout.Label("Simulation Controls", GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Time Scale:", GUILayout.Width(80));
            simulator.timeScale = GUILayout.HorizontalSlider(simulator.timeScale, 0.1f, 10f, GUILayout.Width(150));
            GUILayout.Label(simulator.timeScale.ToString("F2"), GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Distance Scale:", GUILayout.Width(80));
            simulator.distanceScale = GUILayout.HorizontalSlider(simulator.distanceScale, 1e-12f, 1e-6f, GUILayout.Width(150));
            GUILayout.Label(simulator.distanceScale.ToString("E2"), GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Size Scale:", GUILayout.Width(80));
            simulator.sizeScale = GUILayout.HorizontalSlider(simulator.sizeScale, 1e-6f, 1e-2f, GUILayout.Width(150));
            GUILayout.Label(simulator.sizeScale.ToString("E2"), GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Reset Simulation"))
            {
                simulator.ResetSimulation();
            }
            
            if (GUILayout.Button(simulator.enableOrbits ? "Disable Orbits" : "Enable Orbits"))
            {
                simulator.ToggleOrbits();
            }
            
            if (GUILayout.Button(simulator.enableRotation ? "Disable Rotation" : "Enable Rotation"))
            {
                simulator.ToggleRotation();
            }
        }

        GUILayout.Space(10);

        // Orbit controls
        if (orbitManager != null)
        {
            GUILayout.Label("Orbit Controls", GUI.skin.box);
            
            if (GUILayout.Button(orbitManager.showAllOrbits ? "Hide Orbits" : "Show Orbits"))
            {
                orbitManager.ToggleAllOrbits();
            }
            
            if (GUILayout.Button("Recalculate Orbits"))
            {
                orbitManager.RecalculateAllOrbits();
            }
        }

        GUILayout.Space(10);

        // Help
        GUILayout.Label("Controls Help", GUI.skin.box);
        GUILayout.Label("Tab - Toggle UI");
        GUILayout.Label("R - Reset Simulation");
        GUILayout.Label("O - Toggle Orbits");
        GUILayout.Label("T - Toggle Rotation");
        GUILayout.Label("+/- - Speed Up/Down");
        GUILayout.Label("Mouse - Camera Control");
        GUILayout.Label("Wheel - Zoom");

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
