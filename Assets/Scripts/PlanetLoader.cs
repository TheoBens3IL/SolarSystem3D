using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Charge des planètes depuis un fichier CSV/Excel et instancie des PlanetControl.
/// </summary>
public class PlanetLoader : MonoBehaviour
{
    [Header("Fichier CSV/Excel")]
    [Tooltip("Chemin vers le CSV ou Excel exporté avec les KeplerElements.")]
    public string FilePath = "Assets/Ressources/PlanetsData.csv";

    private void Start()
    {
        LoadPlanetsFromCSV(FilePath);
    }

    private void LoadPlanetsFromCSV(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"[PlanetLoader] Fichier non trouvé : {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        if (lines.Length < 3)
        {
            Debug.LogWarning("[PlanetLoader] Fichier vide ou header manquant.");
            return;
        }

        for (int i = 2; i < lines.Length; i++) // saute la première ligne (header)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = line.Split(',');

            try
            {
                // Assumes CSV columns: Name,a,e,i,Omega,omega,M0,epochSeconds,diameter,mass
                PlanetControl.KeplerElements kepler = new PlanetControl.KeplerElements
                {
                    a = double.Parse(fields[3]),
                    e = double.Parse(fields[4]),
                    iDeg = double.Parse(fields[5]),
                    OmegaDeg = double.Parse(fields[6]),
                    omegaDeg = double.Parse(fields[7]),
                    M0Deg = double.Parse(fields[8]),
                    epochSeconds = double.Parse(fields[9])
                };

                string planetName = fields[0];
                double diameter = double.Parse(fields[2]);
                double mass = double.Parse(fields[1]);

                InstantiatePlanet(planetName, kepler, diameter, mass);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlanetLoader] Erreur sur la ligne {i + 1} : {ex.Message}");
            }
        }
    }

    private void InstantiatePlanet(string name, PlanetControl.KeplerElements kepler, double diameter, double mass)
    {
        // Cherche la planète déjà dans la scène
        GameObject planetGO = GameObject.Find(name);
        if (planetGO == null)
        {
            Debug.LogWarning($"[PlanetLoader] Planète {name} non trouvée dans la scène !");
            return;
        }

        PlanetControl pc = planetGO.GetComponent<PlanetControl>();
        if (pc == null)
        {
            Debug.LogWarning($"[PlanetLoader] PlanetControl non trouvé sur {name} !");
            return;
        }

        pc.SetKeplerElements(kepler);
        pc.SetPlanetDiameter(diameter);
        // pc.SetPlanetMass(mass); // si tu as une méthode pour la masse
    }
}
