using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class control : MonoBehaviour
{
    [System.Serializable]
    public class ControlRod
    {
        public Transform rodTransform;
        public bool isInserted = false;
        public float neutronAbsorptionEfficiency; // Eficiencia de absorción basada en el cadmio
    }

    public List<ControlRod> controlRods;
    public float dropDistance = 2f;
    public float speed = 2f;
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();

    // Propiedades reales del Cadmio
    private const float sectionCross = 2450f; // Sección eficaz de absorción en barns (Cadmio)
    private const float density = 8.65f; // g/cm³
    private const float thermalConductivity = 96.6f; // W/(m·K)
    private const float specificHeat = 0.231f; // J/(g·K)

    void Start()
    {
        foreach (ControlRod rod in controlRods)
        {
            originalPositions[rod.rodTransform] = rod.rodTransform.position;
            rod.neutronAbsorptionEfficiency = CalculateAbsorptionEfficiency();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) LowerOneRod();
        if (Input.GetKeyDown(KeyCode.X)) RaiseOneRod();
        if (Input.GetKeyDown(KeyCode.C)) LowerAllRods();
        if (Input.GetKeyDown(KeyCode.V)) RaiseAllRods();
    }

    private float CalculateAbsorptionEfficiency()
    {
        // Sección eficaz del cadmio en barns -> Conversión a metros cuadrados
        float sectionCross_m2 = sectionCross * 1e-28f; // 1 barn = 10⁻²⁸ m²

        // Fórmula básica para determinar eficiencia relativa de absorción de neutrones
        float efficiency = sectionCross_m2 * density;
        return Mathf.Clamp01(efficiency); // Normalizamos entre 0 y 1
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
