using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitDrawer : MonoBehaviour
{
    [Header("Orbit Settings")]
    public int orbitSegments = 100;
    public float lineWidth = 0.1f;
    public Color orbitColor = Color.white;
    public Material orbitMaterial;
    
    [Header("Planet Reference")]
    public PlanetControl planetControl;
    
    [Header("Debug")]
    public bool showOrbit = true;
    public bool updateOrbit = false;

    private LineRenderer lineRenderer;
    private List<Vector3> orbitPoints = new List<Vector3>();
    private bool orbitCalculated = false;

    void Start()
    {
        InitializeLineRenderer();
        
        if (planetControl != null)
        {
            CalculateOrbit();
        }
    }

    void Update()
    {
        if (updateOrbit && planetControl != null)
        {
            CalculateOrbit();
        }
    }

    private void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        if (orbitMaterial != null)
        {
            lineRenderer.material = orbitMaterial;
        }
        else
        {
            // Create a simple material
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = orbitColor;
            lineRenderer.material = mat;
        }
        
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = showOrbit;
    }

    public void CalculateOrbit()
    {
        if (planetControl == null || planetControl.planetData == null)
        {
            Debug.LogWarning("PlanetControl or planet data not assigned!");
            return;
        }

        orbitPoints.Clear();
        PlanetData data = planetControl.planetData;
        float distanceScale = planetControl.distanceScale;

        // Calculate orbit points
        for (int i = 0; i <= orbitSegments; i++)
        {
            float angle = (float)i / orbitSegments * 2f * Mathf.PI;
            Vector3d orbitalPosition = CalculateOrbitalPosition(angle, data);
            Vector3d worldPosition = TransformTo3DSpace(orbitalPosition, data);
            Vector3 scaledPosition = (worldPosition * distanceScale).ToVector3();
            
            orbitPoints.Add(scaledPosition);
        }

        // Update line renderer
        lineRenderer.positionCount = orbitPoints.Count;
        lineRenderer.SetPositions(orbitPoints.ToArray());
        
        orbitCalculated = true;
    }

    private Vector3d CalculateOrbitalPosition(float angle, PlanetData data)
    {
        double a = data.semiMajorAxis;
        double e = data.eccentricity;
        
        // For elliptical orbits, we need to solve Kepler's equation
        // For simplicity, we'll use a parametric approach
        double cosAngle = System.Math.Cos(angle);
        double sinAngle = System.Math.Sin(angle);
        
        // Parametric equations for ellipse
        double x = a * (cosAngle - e);
        double y = a * System.Math.Sqrt(1 - e * e) * sinAngle;
        
        return new Vector3d(x, y, 0);
    }

    private Vector3d TransformTo3DSpace(Vector3d orbitalVector, PlanetData data)
    {
        // Convert orbital elements to rotation matrix
        double i = PlanetData.DegreesToRadians(data.inclination);
        double omega = PlanetData.DegreesToRadians(data.longitudeOfAscendingNode);
        double w = PlanetData.DegreesToRadians(data.argumentOfPeriapsis);

        // Rotation matrices
        double cosOmega = System.Math.Cos(omega);
        double sinOmega = System.Math.Sin(omega);
        double cosI = System.Math.Cos(i);
        double sinI = System.Math.Sin(i);
        double cosW = System.Math.Cos(w);
        double sinW = System.Math.Sin(w);

        // Combined rotation matrix: R = Rz(Ω) * Rx(i) * Rz(ω)
        double[,] R = new double[3, 3];
        
        R[0, 0] = cosOmega * cosW - sinOmega * cosI * sinW;
        R[0, 1] = -cosOmega * sinW - sinOmega * cosI * cosW;
        R[0, 2] = sinOmega * sinI;
        
        R[1, 0] = sinOmega * cosW + cosOmega * cosI * sinW;
        R[1, 1] = -sinOmega * sinW + cosOmega * cosI * cosW;
        R[1, 2] = -cosOmega * sinI;
        
        R[2, 0] = sinI * sinW;
        R[2, 1] = sinI * cosW;
        R[2, 2] = cosI;

        // Apply rotation
        double x = R[0, 0] * orbitalVector.x + R[0, 1] * orbitalVector.y + R[0, 2] * orbitalVector.z;
        double y = R[1, 0] * orbitalVector.x + R[1, 1] * orbitalVector.y + R[1, 2] * orbitalVector.z;
        double z = R[2, 0] * orbitalVector.x + R[2, 1] * orbitalVector.y + R[2, 2] * orbitalVector.z;

        return new Vector3d(x, y, z);
    }

    public void SetOrbitColor(Color color)
    {
        orbitColor = color;
        if (lineRenderer != null)
        {
            lineRenderer.material.color = color;
        }
    }

    public void SetLineWidth(float width)
    {
        lineWidth = width;
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }

    public void SetOrbitSegments(int segments)
    {
        orbitSegments = segments;
        if (orbitCalculated)
        {
            CalculateOrbit();
        }
    }

    public void ToggleOrbit()
    {
        showOrbit = !showOrbit;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = showOrbit;
        }
    }

    public void SetPlanetControl(PlanetControl planet)
    {
        planetControl = planet;
        if (planet != null)
        {
            CalculateOrbit();
        }
    }

    void OnDrawGizmos()
    {
        if (showOrbit && orbitPoints.Count > 0)
        {
            Gizmos.color = orbitColor;
            for (int i = 0; i < orbitPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(orbitPoints[i], orbitPoints[i + 1]);
            }
        }
    }
}
