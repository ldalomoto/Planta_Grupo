using UnityEngine;

public class ReactorCore : MonoBehaviour
{
    public control control; // Referencia a la gestión de las barras de control
    public float reactorPower = 100f; // Potencia del reactor en porcentaje (100% = reactor a potencia máxima)
    public WaterSimulation water;  // Referencia al objeto WaterSimulation que controla el agua

    void Update()
    {
        // Calcular y actualizar la potencia del reactor
        CalculateReactorPower();

        // Actualiza el nivel del agua según la presión calculada
        if (water != null)
        {
            water.SetWaterLevel(GetPressure() * 0.05f); // Control del nivel del agua basado en la presión
        }
    }

    void CalculateReactorPower()
    {
        float totalAbsorption = 0f;
        foreach (var rod in control.controlRods)
        {
            if (rod.isInserted)
            {
                totalAbsorption += control.GetAbsorptionEfficiency();
            }
        }

        // Calcula la potencia del reactor basada en la eficiencia de absorción de las barras de control
        reactorPower = Mathf.Clamp(100f - (totalAbsorption * 100f / control.controlRods.Count), 0f, 100f);
        Debug.Log("Potencia del Reactor: " + reactorPower + "%");
    }

    // Método para calcular la presión del reactor (en función de la potencia)
    public float GetPressure()
    {
        // Simula una presión proporcional a la potencia del reactor
        return Mathf.Clamp(reactorPower * 0.8f, 0f, 100f);
    }
}
