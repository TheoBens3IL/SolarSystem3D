using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Lit un fichier CSV ou Excel contenant les données des planètes et met à jour les PlanetControl dans la scène.
/// </summary>
public class PlanetLoader : MonoBehaviour
{
    [Header("Fichier CSV/Excel")]
    [Tooltip("Chemin vers le CSV ou Excel contenant les KeplerElements des planètes.")]
    public string FilePath = "Assets/Ressources/PlanetsData.csv";

    private void Start()
    {
        LoadPlanetsFromCSV(FilePath);
    }

    /// <summary>
    /// Lit le fichier CSV et instancie/initialise les planètes correspondantes.
    /// </summary>
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

        // Commence à la ligne 2 pour ignorer l'entête
        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = line.Split(',');

            try
            {
                string planetName = fields[0];
                string planetType = fields[18];

                // Colonnes CSV : Name,a,e,i,Omega,omega,M0,epochSeconds,diameter,mass,...
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

                double diameter = double.Parse(fields[2]);
                double mass = double.Parse(fields[1]);

                InstantiatePlanet(planetName, planetType, kepler, diameter, mass);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlanetLoader] Erreur sur la ligne {i + 1} : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Met à jour un PlanetControl existant avec les données lues depuis le CSV.
    /// </summary>
    private void InstantiatePlanet(string name, string type, PlanetControl.KeplerElements kepler, double diameter, double mass)
    {
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
        pc.SetPlanetType(type);
        // Si une méthode SetPlanetMass existe, on pourrait l'appeler ici : pc.SetPlanetMass(mass);
    }
}
