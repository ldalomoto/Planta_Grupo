using UnityEngine;

public class TubeSimulation : MonoBehaviour
{
    public GameObject arrowPrefab; // Prefab de las flechas
    public Transform[] pathPoints; // Puntos del trayecto (entry -> exit)
    public Material tubeMaterial; // Material del tubo
    public Color coldWaterColor = Color.blue; // Color para agua fría
    public Color hotWaterColor = Color.red; // Color para agua caliente
    public float arrowSpeed = 2f; // Velocidad de las flechas
    public float arrowSpawnInterval = 0.5f; // Intervalo de aparición de flechas

    private float spawnTimer = 0f;
    private bool isHotWater = false; // Estado actual del agua (fría o caliente)

    void Start()
    {
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogError("Los puntos del trayecto no están configurados.");
        }
        if (tubeMaterial == null)
        {
            Debug.LogError("Por favor, asigna el material del tubo.");
        }
    }

    void Update()
    {
        // Cambiar el color del tubo según el estado del agua
        tubeMaterial.color = isHotWater ? hotWaterColor : coldWaterColor;

        // Crear flechas a intervalos regulares
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= arrowSpawnInterval)
        {
            SpawnArrow();
            spawnTimer = 0f;
        }
    }

    void SpawnArrow()
    {
        if (pathPoints.Length < 2)
        {
            Debug.LogWarning("Se necesitan al menos dos puntos para mover las flechas.");
            return;
        }

        // Instanciar una flecha en el primer punto del trayecto
        GameObject arrow = Instantiate(arrowPrefab, pathPoints[0].position, Quaternion.identity);
        arrow.AddComponent<ArrowMover>().Initialize(pathPoints, arrowSpeed);
    }

    public void SetWaterState(bool isHot)
    {
        // Cambiar entre agua caliente y fría
        isHotWater = isHot;
    }
}

public class ArrowMover : MonoBehaviour
{
    private Transform[] pathPoints;
    private float speed;
    private int currentPointIndex = 0;

    public void Initialize(Transform[] points, float moveSpeed)
    {
        pathPoints = points;
        speed = moveSpeed;
    }

    void Update()
    {
        if (pathPoints == null || pathPoints.Length == 0) return;

        // Mover la flecha hacia el siguiente punto
        Transform targetPoint = pathPoints[currentPointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        // Cambiar al siguiente punto si se alcanzó el actual
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPointIndex++;
            if (currentPointIndex >= pathPoints.Length)
            {
                Destroy(gameObject); // Destruir la flecha al llegar al final
            }
        }
    }
}
