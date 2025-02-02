using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlRodManager : MonoBehaviour
{
    public List<Transform> controlRods; // Lista de barras de control
    public float dropDistance = 2f; // Distancia de bajada
    public float speed = 2f; // Velocidad de movimiento

    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Queue<Transform> loweredRods = new Queue<Transform>(); // Barras que han bajado

    void Start()
    {
        // Guardar la posición original de cada barra
        foreach (Transform rod in controlRods)
        {
            originalPositions[rod] = rod.position;
        }
    }

    void Update()
    {
        // Bajar una barra por vez
        if (Input.GetKeyDown(KeyCode.Z))
        {
            LowerOneRod();
        }

        // Subir una barra por vez
        if (Input.GetKeyDown(KeyCode.X))
        {
            RaiseOneRod();
        }

        // Bajar todas las barras
        if (Input.GetKeyDown(KeyCode.C))
        {
            LowerAllRods();
        }

        // Subir todas las barras
        if (Input.GetKeyDown(KeyCode.V))
        {
            RaiseAllRods();
        }
    }

    void LowerOneRod()
    {
        foreach (Transform rod in controlRods)
        {
            if (!loweredRods.Contains(rod)) // Si aún no ha bajado
            {
                Vector3 targetPosition = originalPositions[rod] - new Vector3(0, dropDistance, 0);
                StartCoroutine(MoveRod(rod, targetPosition));
                loweredRods.Enqueue(rod);
                break; // Solo baja una barra por pulsación
            }
        }
    }

    void RaiseOneRod()
    {
        if (loweredRods.Count > 0)
        {
            Transform rod = loweredRods.Dequeue();
            StartCoroutine(MoveRod(rod, originalPositions[rod]));
        }
    }

    void LowerAllRods()
    {
        foreach (Transform rod in controlRods)
        {
            Vector3 targetPosition = originalPositions[rod] - new Vector3(0, dropDistance, 0);
            StartCoroutine(MoveRod(rod, targetPosition));
            if (!loweredRods.Contains(rod))
            {
                loweredRods.Enqueue(rod);
            }
        }
    }

    void RaiseAllRods()
    {
        while (loweredRods.Count > 0)
        {
            Transform rod = loweredRods.Dequeue();
            StartCoroutine(MoveRod(rod, originalPositions[rod]));
        }
    }

    IEnumerator MoveRod(Transform rod, Vector3 targetPosition)
    {
        while (Vector3.Distance(rod.position, targetPosition) > 0.01f)
        {
            rod.position = Vector3.MoveTowards(rod.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        rod.position = targetPosition; // Asegurar que quede en la posición exacta
    }
}
