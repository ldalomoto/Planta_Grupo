/*
using UnityEngine;
using System.Collections;

public class NeutronSimulation : MonoBehaviour
{
    public float baseNeutronProduction = 1000f; // Producción base de neutrones
    private float currentNeutronProduction;
    private float neutronAbsorption;
    private float reactorPower = 100f; // Potencia inicial del reactor (100% = sin control)
    private float lastDebugTime = 0f; // Para controlar el tiempo del debug

    public control controlScript; // Referencia al script de control de barras

    void Start()
    {
        currentNeutronProduction = baseNeutronProduction;
    }

    void Update()
    {
        CalculateNeutronBalance();
        
        if (Time.time - lastDebugTime >= 5f) // Mostrar datos cada 5 segundos
        {
            int insertedRods = controlScript.controlRods.FindAll(r => r.isInserted).Count;
            int totalRods = controlScript.controlRods.Count;

            Debug.Log($"[Neutrones] Generados: {currentNeutronProduction} | Absorbidos: {neutronAbsorption} | Potencia: {reactorPower}% | Barras insertadas: {insertedRods}/{totalRods}");
            
            lastDebugTime = Time.time;
        }
    }

    void CalculateNeutronBalance()
    {
        if (controlScript != null)
        {
            // Obtener la absorción de neutrones desde el script de control
            neutronAbsorption = controlScript.controlRods.FindAll(r => r.isInserted).Count * controlScript.absorptionEfficiency * 10f;

            // Ajustar la generación de neutrones según la potencia del reactor
            currentNeutronProduction = Mathf.Max(baseNeutronProduction - neutronAbsorption, 0);

            // Ajustar la potencia del reactor con base en los neutrones
            reactorPower = Mathf.Clamp(100f - (neutronAbsorption / baseNeutronProduction * 100f), 20f, 80f);
        }
    }
}
*/