using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlanetData
{
    public string name;
    public float diameter;
    public float mass;
    public float initialDistance;
    public Vector3 initialPosition;
    public Vector3 initialVelocity;
    public float rotationPeriod;
}

public class PlanetCSVLoader : MonoBehaviour
{
    [Header("CSV Settings")]
    [Tooltip("Nom du fichier CSV (placer le fichier dans Resources/)")]
    public string csvFileName = "PlanetsData";

    [Tooltip("Facteur d’échelle pour adapter les distances Unity")]
    public float scaleFactor = 1f;

    [HideInInspector]
    public List<PlanetData> planets = new List<PlanetData>();

    void Awake()
    {
        LoadCSV();
        ApplyToExistingPlanets();
    }

    /// <summary>
    /// Charge le CSV et remplit la liste de PlanetData
    /// </summary>
    void LoadCSV()
    {
        TextAsset csvData = Resources.Load<TextAsset>(csvFileName);

        if (csvData == null)
        {
            Debug.LogError("CSV introuvable dans Resources : " + csvFileName);
            return;
        }

        string[] lines = csvData.text.Split('\n');

        // Sauter l'entête
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] values = lines[i].Split('\t'); // Séparateur CSV : tabulation

            PlanetData data = new PlanetData();
            data.name = values[0];
            data.diameter = float.Parse(values[1]);
            data.mass = float.Parse(values[2]);
            data.initialDistance = float.Parse(values[3]);

            // Positions
            float posX = float.Parse(values[4]);
            float posY = float.Parse(values[5]);
            float posZ = float.Parse(values[6]);
            data.initialPosition = new Vector3(posX, posY, posZ) * scaleFactor;

            // Vitesses
            float velX = float.Parse(values[7]);
            float velY = float.Parse(values[8]);
            float velZ = float.Parse(values[9]);
            data.initialVelocity = new Vector3(velX, velY, velZ) * scaleFactor;

            // Rotation
            data.rotationPeriod = float.Parse(values[10]);

            planets.Add(data);
        }

        Debug.Log("Chargement CSV terminé : " + planets.Count + " planètes trouvées.");
    }

    /// <summary>
    /// Applique les données du CSV aux planètes déjà présentes dans la scène
    /// </summary>
    void ApplyToExistingPlanets()
    {
        foreach (PlanetData data in planets)
        {
            GameObject planetObj = GameObject.Find(data.name);

            if (planetObj != null)
            {
                PlanetControl control = planetObj.GetComponent<PlanetControl>();

                if (control != null)
                {
                    control.Initialize(data);
                    Debug.Log($"✅ Données appliquées à la planète {data.name}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Le GameObject {data.name} n’a pas de script PlanetControl attaché.");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Planète {data.name} du CSV non trouvée dans la scène.");
            }
        }
    }
}
