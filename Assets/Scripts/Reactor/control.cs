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
        public float neutronAbsorptionEfficiency = 0.006f;
        public int rodID = 0;
    }

    public float dropDistance = 45f;
    public float speed = 5f;


    public List<ControlRod> controlRods;
    private const float cadmiumMicroXS = 5f;
    private const float cadmiumDensity = 8.65f;
    private const float molarMassCd = 112.41f;
    private const float barnToCm2 = 1e-24f;
    private const float avogadro = 6.022e23f;

    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Queue<ControlRod> loweredRods = new Queue<ControlRod>();

    void Start()
    {
        int rodCount = 0;
        foreach (ControlRod rod in controlRods)
        {
            rod.rodID = rodCount++;
            originalPositions[rod.rodTransform] = rod.rodTransform.position;
            rod.neutronAbsorptionEfficiency = CalculateAbsorptionEfficiency(400f);
        }
    }

    public float CalculateAbsorptionEfficiency(float rodLengthCM)
    {
        float sigma = cadmiumMicroXS * barnToCm2;
        float N = (cadmiumDensity * avogadro) / molarMassCd;
        float macroXS = sigma * N;
        return 1f - Mathf.Exp(-macroXS * rodLengthCM);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) LowerOneRod();
        if (Input.GetKeyDown(KeyCode.X)) RaiseOneRod();
        if (Input.GetKeyDown(KeyCode.C)) LowerAllRods();
        if (Input.GetKeyDown(KeyCode.V)) RaiseAllRods();
    }

    void LowerOneRod()
    {
        foreach (ControlRod rod in controlRods)
        {
            if (!rod.isInserted)
            {
                Vector3 targetPosition = originalPositions[rod.rodTransform] - new Vector3(0, dropDistance, 0);
                StartCoroutine(MoveRod(rod, targetPosition));
                rod.isInserted = true;
                loweredRods.Enqueue(rod);
                break;
            }
        }
    }

    void RaiseOneRod()
    {
        if (loweredRods.Count > 0)
        {
            ControlRod rod = loweredRods.Dequeue();
            StartCoroutine(MoveRod(rod, originalPositions[rod.rodTransform]));
            rod.isInserted = false;
        }
    }

    void LowerAllRods()
    {
        foreach (ControlRod rod in controlRods)
        {
            if (!rod.isInserted)
            {
                Vector3 targetPosition = originalPositions[rod.rodTransform] - new Vector3(0, dropDistance, 0);
                StartCoroutine(MoveRod(rod, targetPosition));
                rod.isInserted = true;
                loweredRods.Enqueue(rod);
            }
        }
    }

    void RaiseAllRods()
    {
        while (loweredRods.Count > 0)
        {
            ControlRod rod = loweredRods.Dequeue();
            StartCoroutine(MoveRod(rod, originalPositions[rod.rodTransform]));
            rod.isInserted = false;
        }
    }

    public int GetInsertedRodCount()
    {
        int count = 0;
        foreach(ControlRod rod in controlRods){
            if(rod.isInserted) count++;
        }
        return count;
    }

    IEnumerator MoveRod(ControlRod rod, Vector3 targetPosition)
    {
        while (Vector3.Distance(rod.rodTransform.position, targetPosition) > 0.01f)
        {
            rod.rodTransform.position = Vector3.MoveTowards(rod.rodTransform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        rod.rodTransform.position = targetPosition;
    }
}



/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class control : MonoBehaviour
{
    [System.Serializable]
    public class ControlRod
    {
        public Transform rodTransform;
        public bool isInserted = false;
        public float neutronAbsorptionEfficiency = 0.006f;
        public int rodID = 0;
        public float dropDistance = 2f;
        public float speed = 2f;
    }

    public List<ControlRod> controlRods;
    //public float dropDistance = 2f;
    //public float speed = 2f;

    // Parámetros físicos reales del Cadmio
    private const float cadmiumMicroXS = 5f;        // barns
    private const float cadmiumDensity = 8.65f;     // g/cm³
    private const float molarMassCd = 112.41f;      // g/mol
    private const float barnToCm2 = 1e-24f;
    private const float avogadro = 6.022e23f;

    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Queue<Transform> loweredRods = new Queue<Transform>(); // Barras que han bajado

    void Start()
    {
        int rodCount = 0;
        foreach (ControlRod rod in controlRods)
        {
            rod.rodID = rodCount++;
            originalPositions[rod.rodTransform] = rod.rodTransform.position;
            rod.neutronAbsorptionEfficiency = CalculateAbsorptionEfficiency(400f); // 4 metros
            //rod.neutronAbsorptionEfficiency = Mathf.Clamp(CalculateAbsorptionEfficiency(400f), 0.8f, 1f);
        }
    }

    public float CalculateAbsorptionEfficiency(float rodLengthCM)
    {
        float sigma = cadmiumMicroXS * barnToCm2;
        float N = (cadmiumDensity * avogadro) / molarMassCd;
        float macroXS = sigma * N;
        return 1f - Mathf.Exp(-macroXS * rodLengthCM);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) LowerOneRod();
        if (Input.GetKeyDown(KeyCode.X)) RaiseOneRod();
        if (Input.GetKeyDown(KeyCode.C)) LowerAllRods();
        if (Input.GetKeyDown(KeyCode.V)) RaiseAllRods();
    }

    //void LowerOneRod() => StartCoroutine(MoveFirstAvailableRod(down: true));
    //void RaiseOneRod() => StartCoroutine(MoveFirstAvailableRod(down: false));

    IEnumerator MoveFirstAvailableRod(bool down)
    {
        foreach (ControlRod rod in controlRods)
        {
            if (down && !rod.isInserted)
            {
                Vector3 target = originalPositions[rod.rodTransform] - Vector3.up * dropDistance;
                yield return StartCoroutine(MoveRod(rod.rodTransform, target));
                rod.isInserted = true;
                break;
            }
            else if (!down && rod.isInserted)
            {
                yield return StartCoroutine(MoveRod(rod.rodTransform, originalPositions[rod.rodTransform]));
                rod.isInserted = false;
                break;
            }
        }
    }

    void LowerOneRod()
    {
        foreach (ControlRod rod in controlRods)
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

    void LowerAllRods()
    {
        foreach (ControlRod rod in controlRods)
        {
            if (!rod.isInserted)
            {
                Vector3 target = originalPositions[rod.rodTransform] - Vector3.up * dropDistance;
                StartCoroutine(MoveRod(rod.rodTransform, target));
                rod.isInserted = true;
            }
        }
    }

    void RaiseAllRods()
    {
        foreach (ControlRod rod in controlRods)
        {
            if (rod.isInserted)
            {
                StartCoroutine(MoveRod(rod.rodTransform, originalPositions[rod.rodTransform]));
                rod.isInserted = false;
            }
        }
    }

    IEnumerator MoveRod(Transform rod, Vector3 targetPosition)
    {
        while (Vector3.Distance(rod.position, targetPosition) > 0.01f)
        {
            rod.position = Vector3.MoveTowards(rod.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        rod.position = targetPosition;
    }
}
*/