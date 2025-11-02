using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Permet d’ouvrir ou fermer un panel UI avec un clic.
/// Attache ce script à un bouton ou élément cliquable.
/// </summary>
public class OpenPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private GameObject panel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Appelé lorsque l'utilisateur clique sur l'objet.
    /// Inverse l'état "Display" de l'Animator attaché au panel.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Animator animation = panel.GetComponent<Animator>();
        if (animation != null)
        {
            bool isOpen = animation.GetBool("Display");
            animation.SetBool("Display", !isOpen);
        }
    }
}
