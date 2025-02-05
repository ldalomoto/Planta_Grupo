using UnityEngine;

public class WaterSimulation : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    public float waterLevel = 5f;  // Altura inicial del agua
    public float maxWaterLevel = 10f; // Altura máxima del agua
    public float waveHeight = 0.05f; // Altura de las olas
    public float waveSpeed = 2.0f; // Velocidad de las olas

    void Start()
    {
        // Asegurar que el objeto tiene un MeshFilter y un MeshRenderer
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("No se encontró MeshFilter en el objeto de agua.");
            return;
        }

        mesh = meshFilter.mesh;
        mesh.MarkDynamic();
        if (mesh == null)
        {
            Debug.LogError("No se encontró la malla en el MeshFilter.");
            return;
        }

        originalVertices = mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i];
        }

        // Asegurar que la malla tenga normales correctas
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void Update()
    {
        if (mesh == null) return;

        SetWaterLevel(waterLevel);
        SimulateWaves();

        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();  // Asegurar que la malla tiene límites correctos
    }

    public void SetWaterLevel(float newLevel)
    {
        waterLevel = Mathf.Clamp(newLevel, 0f, maxWaterLevel);

        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i]; // Restauramos los valores originales
            modifiedVertices[i].y = originalVertices[i].y + waterLevel; 
        }
    }

    void SimulateWaves() {
        for (int i = 0; i < modifiedVertices.Length; i++) {
            Vector3 worldPos = transform.TransformPoint(originalVertices[i]);
            modifiedVertices[i].y += Mathf.Sin(Time.time * waveSpeed + worldPos.x + worldPos.z) * waveHeight;
        }
    }

    public void ChangeWaterLevel(float delta)
    {
        waterLevel += delta;
    }
}
