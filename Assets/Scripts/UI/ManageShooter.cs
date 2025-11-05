using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Gère l’icône 3D du shooter dans l’UI.
/// La rotation du modèle est activée uniquement lorsque le curseur survole l’élément.
/// </summary>
public class ManageShooter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Références")]
    [SerializeField] private GameObject shooterModel; // Le modèle 3D à faire tourner

    [Header("Paramètres de rotation")]
    [SerializeField] private float rotationSpeed = 50f; // vitesse de rotation en degrés par seconde

    private bool isHovering = false; // true lorsque la souris est au-dessus

    private void Update()
    {
        // Rotation continue du modèle uniquement lors du survol
        if (isHovering && shooterModel != null)
        {
            shooterModel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// Appelé quand le curseur entre dans la zone du UI element
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Survol détecté !");
        isHovering = true;
    }

    /// <summary>
    /// Appelé quand le curseur quitte la zone du UI element
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Fin du survol !");
        isHovering = false;
    }
}
