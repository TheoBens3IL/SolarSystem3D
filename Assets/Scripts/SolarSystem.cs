using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    [System.Serializable]
    public class Planete
    {
        public string nom;
        public Transform objet;    // Le GameObject planète
        public float demiGrandAxe; // distance moyenne au Soleil (en UA ou unité arbitraire)
        public float excentricite; // 0 = cercle parfait
        public float periode;      // en années terrestres ou secondes simulées
        [HideInInspector] public LineRenderer orbite;
        [HideInInspector] public float angle;
    }

    public Transform soleil;
    public Planete[] planetes;
    public int resolutionOrbite = 100;
    public float vitesseTemps = 10f; // accélération du temps

    void Start()
    {
        foreach (var p in planetes)
        {
            // Ajouter LineRenderer si pas déjà présent
            p.orbite = p.objet.gameObject.AddComponent<LineRenderer>();
            p.orbite.positionCount = resolutionOrbite + 1;
            p.orbite.widthMultiplier = 0.02f;
            p.orbite.loop = true;

            // Dessiner l’orbite
            for (int i = 0; i <= resolutionOrbite; i++)
            {
                float theta = 2f * Mathf.PI * i / resolutionOrbite;
                Vector3 pos = PositionOrbite(p, theta);
                p.orbite.SetPosition(i, pos);
            }
        }
    }

    void Update()
    {
        foreach (var p in planetes)
        {
            // Avancer l’angle
            float vitesseAngulaire = 2f * Mathf.PI / p.periode;
            p.angle += vitesseAngulaire * Time.deltaTime * vitesseTemps;

            // Nouvelle position
            Vector3 pos = PositionOrbite(p, p.angle);
            p.objet.position = soleil.position + pos;
        }
    }

    Vector3 PositionOrbite(Planete p, float angle)
    {
        // Formule ellipse (plan XY)
        float a = p.demiGrandAxe;
        float e = p.excentricite;
        float b = a * Mathf.Sqrt(1 - e * e);

        float x = a * Mathf.Cos(angle);
        float z = b * Mathf.Sin(angle);

        return new Vector3(x, 0, z);
    }
}
