using UnityEngine;

public class NeutronGenerator : MonoBehaviour
{
    public GameObject neutronPrefab; // Prefab del neutrón
    public float spawnRate = 0.1f; // Tiempo entre la generación de neutrones
    public int maxNeutrons = 10000; // Límite de neutrones en la simulación
    public float reactorRadius = 1.5f; // Radio del cilindro del núcleo del reactor
    public float reactorHeight = 4f; // Altura del núcleo del reactor
    public float neutronSpeed = 5f; // Velocidad inicial del neutrón

    private int currentNeutrons = 0;

    void Start()
    {
        InvokeRepeating(nameof(SpawnNeutron), 0f, spawnRate);
    }

    void SpawnNeutron()
    {
        if (currentNeutrons >= maxNeutrons) return;

        // Generar posición dentro del cilindro del reactor
        Vector3 spawnPos = GetRandomPositionInCylinder();

        // Crear neutrón
        GameObject neutron = Instantiate(neutronPrefab, spawnPos, Quaternion.identity);
        Rigidbody rb = neutron.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Darle una dirección aleatoria en 3D
            Vector3 randomDirection = Random.onUnitSphere;
            rb.velocity = randomDirection * neutronSpeed;
        }

        currentNeutrons++;
    }

    Vector3 GetRandomPositionInCylinder()
    {
        float radius = Random.Range(0f, reactorRadius);
        float angle = Random.Range(0f, Mathf.PI * 2);
        float height = Random.Range(-reactorHeight / 2, reactorHeight / 2);

        float x = radius * Mathf.Cos(angle);
        float z = radius * Mathf.Sin(angle);
        float y = height;

        return new Vector3(x, y, z) + transform.position;
    }
}
 