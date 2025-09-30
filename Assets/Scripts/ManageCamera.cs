using UnityEngine;

public class ManageCamera : MonoBehaviour
{
    public Camera mainCamera;
    public Camera followCamera;
    public FollowCameraController followScript;

    public SolarSystem solarSystem;
    private Transform selectedPlanet;

    void Update()
    {
        // Sélection de planète avec clic gauche
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                foreach (var p in solarSystem.planetes)
                {
                    if (hit.transform == p.objet)
                    {
                        selectedPlanet = p.objet;
                        followScript.target = selectedPlanet;
                        break;
                    }
                }
            }
        }

        // Appui sur A pour activer la caméra de suivi
        if (selectedPlanet != null && Input.GetKeyDown(KeyCode.A))
        {
            mainCamera.enabled = false;
            followCamera.enabled = true;
        }

        // Appui sur w pour revenir à la caméra libre
        if (Input.GetKeyDown(KeyCode.W))
        {
            followCamera.enabled = false;
            mainCamera.enabled = true;
        }
    }
}
