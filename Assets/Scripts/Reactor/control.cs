using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class control : MonoBehaviour
{
    public enum MaterialType { BoronCarbide, Cadmium, Hafnium } // Tipos de materiales

    public MaterialType selectedMaterial = MaterialType.BoronCarbide; // Material elegido para todas las barras

    [System.Serializable]
    public class ControlRod
    {
        public Transform rodTransform;
        public bool isInserted = false;
    }

    public List<ControlRod> controlRods;
    public float dropDistance = 2f;
    public float speed = 2f;
    public float absorptionEfficiency;
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    

    private float reactorPower = 100f; // Porcentaje de potencia del reactor (100% máximo)
    private const float maxPower = 80f; // Potencia máxima permitida (80%)
    private const float minPower = 20f; // Potencia mínima antes de subir barras
    private float neutronProduction = 1000f; // Neutrones generados (valor base)
    private float neutronAbsorption = 0f; // Neutrones absorbidos
    private float lastDebugTime = 0f; // Para controlar la frecuencia de los mensajes Debug

    void Start()
    {
        foreach (ControlRod rod in controlRods)
        {
            originalPositions[rod.rodTransform] = rod.rodTransform.position;
        }

        // Asignar eficiencia de absorción según el material seleccionado
        switch (selectedMaterial)
        {
            case MaterialType.BoronCarbide:
                absorptionEfficiency = 0.8f;
                break;
            case MaterialType.Cadmium:
                absorptionEfficiency = 0.6f;
                break;
            case MaterialType.Hafnium:
                absorptionEfficiency = 0.7f;
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) LowerOneRod();
        if (Input.GetKeyDown(KeyCode.X)) RaiseOneRod();
        if (Input.GetKeyDown(KeyCode.C)) LowerAllRods();
        if (Input.GetKeyDown(KeyCode.V)) RaiseAllRods();

        CalculateReactorPower();
        AutoAdjustControlRods();

        if (Time.time - lastDebugTime >= 5f) // Debug cada 5 segundos
        {
            int insertedRods = controlRods.FindAll(r => r.isInserted).Count;
            int totalRods = controlRods.Count;

            Debug.Log($"Potencia: {reactorPower}% | Neutrones generados: {neutronProduction} | Neutrones absorbidos: {neutronAbsorption} | Barras insertadas: {insertedRods}/{totalRods}");
            lastDebugTime = Time.time;
        }
    }

    void LowerOneRod()
    {
        foreach (ControlRod rod in controlRods)
        {
            if (!rod.isInserted)
            {
                StartCoroutine(MoveRod(rod.rodTransform, originalPositions[rod.rodTransform] - new Vector3(0, dropDistance, 0)));
                rod.isInserted = true;
                break;
            }
        }
    }

    void RaiseOneRod()
    {
        for (int i = controlRods.Count - 1; i >= 0; i--)
        {
            if (controlRods[i].isInserted)
            {
                StartCoroutine(MoveRod(controlRods[i].rodTransform, originalPositions[controlRods[i].rodTransform]));
                controlRods[i].isInserted = false;
                break;
            }
        }
    }

    void LowerAllRods()
    {
        foreach (ControlRod rod in controlRods)
        {
            StartCoroutine(MoveRod(rod.rodTransform, originalPositions[rod.rodTransform] - new Vector3(0, dropDistance, 0)));
            rod.isInserted = true;
        }
    }

    void RaiseAllRods()
    {
        foreach (ControlRod rod in controlRods)
        {
            StartCoroutine(MoveRod(rod.rodTransform, originalPositions[rod.rodTransform]));
            rod.isInserted = false;
        }
    }

    private void CalculateReactorPower()
    {
        int insertedRods = controlRods.FindAll(r => r.isInserted).Count;
        foreach (ControlRod rod in controlRods)
        {
            if (rod.isInserted) insertedRods++;
        }

        float maxPossibleAbsorption = controlRods.Count * absorptionEfficiency;
        neutronAbsorption = (insertedRods * absorptionEfficiency) / maxPossibleAbsorption * 100f; 

        reactorPower = Mathf.Clamp(100f - neutronAbsorption, minPower, maxPower);
    }

    private void AutoAdjustControlRods()
    {
        if (reactorPower > maxPower) // Si el reactor está por encima del 80%, bajar barras
        {
            foreach (ControlRod rod in controlRods)
            {
                if (!rod.isInserted)
                {
                    StartCoroutine(MoveRod(rod.rodTransform, originalPositions[rod.rodTransform] - new Vector3(0, dropDistance, 0)));
                    rod.isInserted = true;
                    return;
                }
            }
        }
        else if (reactorPower < minPower) // Si está por debajo del mínimo, subir barras
        {
            for (int i = controlRods.Count - 1; i >= 0; i--)
            {
                if (controlRods[i].isInserted)
                {
                    StartCoroutine(MoveRod(controlRods[i].rodTransform, originalPositions[controlRods[i].rodTransform]));
                    controlRods[i].isInserted = false;
                    return;
                }
            }
        }
    }

    private IEnumerator MoveRod(Transform rod, Vector3 targetPosition)
    {
        while (Vector3.Distance(rod.position, targetPosition) > 0.01f)
        {
            rod.position = Vector3.MoveTowards(rod.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        rod.position = targetPosition;
    }
}
