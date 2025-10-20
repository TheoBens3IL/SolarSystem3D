using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlanetDataLoader : MonoBehaviour
{
    [Header("Data Source")]
    public TextAsset csvFile;
    
    [Header("Debug")]
    public bool debugMode = false;

    private List<PlanetData> planetDataList = new List<PlanetData>();

    public List<PlanetData> LoadPlanetData()
    {
        if (planetDataList.Count > 0)
            return planetDataList;

        if (csvFile == null)
        {
            Debug.LogError("CSV file not assigned!");
            return planetDataList;
        }

        string[] lines = csvFile.text.Split('\n');
        
        // Skip header lines (first 2 lines)
        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = ParseCSVLine(line);
            
            if (values.Length > 0 && !string.IsNullOrEmpty(values[0]))
            {
                PlanetData planetData = new PlanetData(values);
                planetDataList.Add(planetData);
                
                if (debugMode)
                {
                    Debug.Log($"Loaded planet: {planetData.name}, " +
                             $"Mass: {planetData.mass:E2} kg, " +
                             $"Semi-major axis: {planetData.semiMajorAxis:E2} m, " +
                             $"Orbital period: {planetData.orbitalPeriod / 86400:F2} days");
                }
            }
        }

        Debug.Log($"Successfully loaded {planetDataList.Count} planets from CSV data.");
        return planetDataList;
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> values = new List<string>();
        bool inQuotes = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.Trim());
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }

        // Add the last value
        values.Add(currentValue.Trim());

        return values.ToArray();
    }

    public PlanetData GetPlanetData(string planetName)
    {
        if (planetDataList.Count == 0)
            LoadPlanetData();

        foreach (var planet in planetDataList)
        {
            if (planet.name.Equals(planetName, System.StringComparison.OrdinalIgnoreCase))
                return planet;
        }

        Debug.LogWarning($"Planet data not found for: {planetName}");
        return null;
    }

    public List<PlanetData> GetAllPlanetData()
    {
        if (planetDataList.Count == 0)
            LoadPlanetData();
        
        return planetDataList;
    }
}
