using UnityEngine;

public class StarControl : MonoBehaviour
{
    [SerializeField] private float starMass = 1.989e30f;
    [SerializeField] private float starDiameter = 1.3927e9f;

    [SerializeField] private float betaRadius = 0.1f;
    [SerializeField] private float gammaRadius = 0.5f;

    private void Awake()
    {
        // Soleil toujours à l'origine
        transform.position = Vector3.zero;
        ApplyVisualScale();
    }

    private void Update()
    {
        // Vérification permanente que le Soleil reste à l'origine
        if (transform.position != Vector3.zero)
        {
            transform.position = Vector3.zero;
        }
    }

    private void ApplyVisualScale()
    {
        float scaledDiameter = betaRadius * Mathf.Pow(starDiameter, gammaRadius);
        transform.localScale = new Vector3(scaledDiameter, scaledDiameter, scaledDiameter);
    }

    public float GetMass() => starMass;
}
