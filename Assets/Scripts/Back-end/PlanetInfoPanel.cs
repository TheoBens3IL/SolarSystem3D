using TMPro;
using UnityEngine;

public class PlanetInfoPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;       // Affiche le nom de la planète
    public TextMeshProUGUI typeText;       // Affiche le type de planète
    public TextMeshProUGUI distanceText;   // Distance au Soleil en km
    public TextMeshProUGUI distanceUaText; // Distance au Soleil en UA
    public TextMeshProUGUI diameterText;   // Diamètre de la planète
    public TextMeshProUGUI periodText;     // Période orbitale en heures

    [Header("Display Settings")]
    public GameObject panelRoot;           // Racine du panel à afficher/masquer

    private void Start()
    {
        // Si nécessaire, on peut démarrer avec le panel fermé
        // panelRoot.SetActive(false);
    }

    /// <summary>
    /// Met à jour le panel avec les informations de la planète.
    /// </summary>
    public void ShowInfo(PlanetControl planet)
    {
        if (planet == null) return;

        // Gérer l'animation d'ouverture du panel si un Animator est attaché
        if (panelRoot != null)
        {
            Animator animation = panelRoot.GetComponent<Animator>();
            if (animation != null)
            {
                bool isOpen = animation.GetBool("Display");
                animation.SetBool("Display", !isOpen);
            }
        }

        // Mise à jour des textes UI
        nameText.text = $"{planet.GetPlanetName()}";
        typeText.text = $" {planet.GetPlanetType()}";
        distanceText.text = $"{(planet.SemiMajorAxis / 1000):F2} Km";
        distanceUaText.text = $"{(planet.SemiMajorAxis / 1.496e11):F2} AU";
        diameterText.text = $"{(planet.PlanetDiameter / 1000f):N0} km";
        periodText.text = $"{planet.OrbitalPeriodDays:F1} Heures";
    }

    // Optionnel : méthode pour cacher le panel si besoin
    // public void HideInfo()
    // {
    //     if (panelRoot != null)
    //         panelRoot.SetActive(false);
    // }
}
