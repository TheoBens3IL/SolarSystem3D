using UnityEngine;

/// <summary>
/// Permet de basculer entre plusieurs caméras à l'aide des flèches gauche et droite.
/// </summary>
public class ManageCameras : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    private int currentIndex = 0;

    void Start()
    {
        // Active uniquement la première caméra au démarrage
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentIndex);
        }
    }

    void Update()
    {
        // Flèche droite : caméra suivante
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextCamera();
        }
        // Flèche gauche : caméra précédente
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousCamera();
        }
    }

    private void NextCamera()
    {
        cameras[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex + 1) % cameras.Length;
        cameras[currentIndex].gameObject.SetActive(true);
    }

    private void PreviousCamera()
    {
        cameras[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex - 1 + cameras.Length) % cameras.Length;
        cameras[currentIndex].gameObject.SetActive(true);
    }
}
