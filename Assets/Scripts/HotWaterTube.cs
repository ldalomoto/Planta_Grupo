using UnityEngine;

public class HotWaterTube : MonoBehaviour
{
    public GameObject arrowPrefab; // Prefab de la flecha
    public Transform[] pathPoints; // Puntos del trayecto (entry -> exit)
    public Material tubeMaterial; // Material del tubo
    public Color hotWaterColor = Color.blue; // Color del tubo para agua fría
    public Color arrowColor = Color.red; // Nuevo color para la flecha
    public float arrowSpeed = 5f; // Velocidad de las flechas
    public float arrowSpawnInterval =1f; // Intervalo de aparición de flechas
    public Vector3 arrowScale = new Vector3(25f, 25f, 25f); // Escala de la flecha

    private float spawnTimer = 0f;

    void Start()
    {
        // Verifica que el material del tubo esté asignado
        if (tubeMaterial == null)
        {
            Debug.LogError("Por favor, asigna el material del tubo.");
            return;
        }

        // Cambia el color inicial del tubo a azul (agua fría)
        tubeMaterial.color = hotWaterColor;
    }

    void Update()
    {
        // Genera flechas a intervalos regulares
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

        // Instancia una flecha en el primer punto del trayecto
        GameObject arrow = Instantiate(arrowPrefab, pathPoints[0].position, Quaternion.identity);

        // Cambiar la escala de la flecha
        arrow.transform.localScale = arrowScale;

        // Inicializa la flecha con el color y velocidad
        arrow.AddComponent<ArrowMoverHot>().Initialize(pathPoints, arrowSpeed, arrowColor);
    }
}

public class ArrowMoverHot : MonoBehaviour
{
    private Transform[] pathPoints;
    private float speed;
    private int currentPointIndex = 0;

    public void Initialize(Transform[] points, float moveSpeed, Color arrowColor)
    {
        pathPoints = points;
        speed = moveSpeed;

        // Cambia el color de la flecha
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = arrowColor;
        }
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
