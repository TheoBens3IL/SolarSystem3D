using UnityEngine;

public class ManageCameras : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    private int currentIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Activate only the first camera at the start 
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextCamera();
        }
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
