using UnityEngine;
using UnityEngine.EventSystems;

public class PlanetIconUI : MonoBehaviour, IPointerClickHandler
{
    public PlanetControl linkedPlanet;
    public PlanetClicCameraFollower cameraFollower;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (linkedPlanet != null && cameraFollower != null)
        {
            cameraFollower.SelectPlanetFromUI(linkedPlanet);
        }
    }
}