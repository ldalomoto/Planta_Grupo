using UnityEngine;

public class WaterFlowWithPath : MonoBehaviour
{
    public GameObject waterParticlePrefab; // Prefab del agua
    public float particleSpeed = 5f; // Velocidad del flujo
    public float particleLifetime = 5f; // Tiempo que vive cada partícula

    private ParticleSystem waterParticles;
    private Transform[] pathPoints; // Puntos de la trayectoria (entryPoint -> exitPoint)
    private int[] currentPathIndices; // Almacena el índice del punto objetivo para cada partícula

    void Start()
    {
        // Buscar todos los puntos de la trayectoria
        pathPoints = GetPathPoints();
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogError("No se encontraron puntos en la trayectoria. Asegúrate de que estén correctamente nombrados.");
            return;
        }

        // Instanciar el sistema de partículas
        if (waterParticlePrefab != null)
        {
            GameObject particleObject = Instantiate(waterParticlePrefab, pathPoints[0].position, Quaternion.identity);
            waterParticles = particleObject.GetComponent<ParticleSystem>();

            // Configurar partículas
            var mainModule = waterParticles.main;
            mainModule.startLifetime = particleLifetime;
            mainModule.startSpeed = 0; // Velocidad se controla manualmente
            mainModule.gravityModifier = 0;

            // Activar colisiones para que las partículas interactúen con los tubos
            var collisionModule = waterParticles.collision;
            collisionModule.enabled = true;
            collisionModule.type = ParticleSystemCollisionType.World;
            collisionModule.collidesWith = LayerMask.GetMask("Tube");
        }
        else
        {
            Debug.LogError("Por favor, asigna el prefab de partículas de agua.");
        }
    }

    void Update()
    {
        // Controlar el movimiento de las partículas
        if (waterParticles != null && pathPoints != null)
        {
            MoveParticlesAlongPath();
        }
    }

    void MoveParticlesAlongPath()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[waterParticles.particleCount];
        int numParticles = waterParticles.GetParticles(particles);

        // Inicializar los índices si aún no lo hemos hecho
        if (currentPathIndices == null || currentPathIndices.Length != numParticles)
        {
            currentPathIndices = new int[numParticles];
            for (int i = 0; i < numParticles; i++)
            {
                currentPathIndices[i] = 0; // Todas las partículas comienzan en el primer punto
            }
        }

        for (int i = 0; i < numParticles; i++)
        {
            // Obtener el índice actual del punto objetivo
            int targetIndex = currentPathIndices[i];

            if (targetIndex < pathPoints.Length)
            {
                // Mover la partícula hacia el punto objetivo
                Vector3 currentPosition = particles[i].position;
                Vector3 targetPosition = pathPoints[targetIndex].position;
                Vector3 direction = (targetPosition - currentPosition).normalized;

                // Depuración: Mostrar dirección y punto objetivo
                Debug.Log($"Partícula {i}: Moviéndose hacia el punto {targetIndex} ({targetPosition}). Dirección: {direction}");

                particles[i].position += direction * particleSpeed * Time.deltaTime;

                // Verificar si la partícula alcanzó el punto objetivo
                if (Vector3.Distance(currentPosition, targetPosition) < 0.1f)
                {
                    currentPathIndices[i]++; // Pasar al siguiente punto
                    Debug.Log($"Partícula {i}: Alcanzó el punto {targetIndex}. Siguiente punto: {currentPathIndices[i]}");
                }
            }
            else
            {
                // Si la partícula llega al último punto, la desactivamos o la reciclamos
                particles[i].remainingLifetime = -1f;
                Debug.Log($"Partícula {i}: Ha alcanzado el último punto. Se elimina.");
            }
        }

        waterParticles.SetParticles(particles, numParticles);
    }

    Transform[] GetPathPoints()
    {
        // Obtener los puntos según los nombres definidos
        GameObject entryPoint = GameObject.Find("entryPoint");
        GameObject exitPoint = GameObject.Find("exitPoint");

        if (entryPoint == null || exitPoint == null)
        {
            Debug.LogError("No se encontraron los puntos entryPoint o exitPoint.");
            return null;
        }

        // Crear una lista para almacenar los puntos en orden
        Transform[] points = new Transform[19]; // entryPoint + 17 puntos + exitPoint
        points[0] = entryPoint.transform;

        // Buscar los puntos intermedios por nombre
        for (int i = 1; i <= 17; i++)
        {
            GameObject point = GameObject.Find(i.ToString());
            if (point == null)
            {
                Debug.LogError($"No se encontró el punto intermedio {i}. Asegúrate de que está correctamente nombrado.");
                return null;
            }
            points[i] = point.transform;
        }

        points[18] = exitPoint.transform;

        return points;
    }
}