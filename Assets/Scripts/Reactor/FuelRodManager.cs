using UnityEngine;
using System.Collections.Generic;

public class FuelRodManager : MonoBehaviour
{
    [System.Serializable]
    public class FuelRod
    {
        public Transform rodTransform;
        public float enrichmentLevel = 0.05f; // Ejemplo: 3% de U-235
        [HideInInspector] public float neutronProduction;
    }

    public List<FuelRod> fuelRods = new List<FuelRod>();
    public ControlRodManager controlRodManager;  // Referencia al sistema de barras de control

    private const float AvogadroNumber = 6.022e23f;  
    private const float UraniumDensity = 19.1f;
    private const float U235AtomicMass = 235.0f;
    private const float FissionCrossSection = 585e-24f; 
    private const float BaseNeutronFlux = 1e12f;

    void Start()
    {
        if (fuelRods == null || fuelRods.Count == 0)
        {
            Debug.LogError("❌ No hay barras de combustible asignadas en FuelRodManager.");
            return;
        }

        if (controlRodManager == null)
        {
            Debug.LogError("❌ No se encontró ControlRodManager. Asigna este script en el Inspector.");
            return;
        }

        InitializeFuelRods();
    }

    void InitializeFuelRods()
    {
        foreach (FuelRod rod in fuelRods)
        {
            if (rod == null || rod.rodTransform == null)
            {
                Debug.LogError("❌ Una de las barras de combustible no tiene asignado un Transform.");
                continue;
            }

            rod.neutronProduction = CalculateNeutronProduction(rod.enrichmentLevel);
        }
    }

    float CalculateNeutronProduction(float enrichment)
    {
        float fuelDensity = (UraniumDensity / U235AtomicMass) * AvogadroNumber;  
        float effectiveU235Density = fuelDensity * enrichment;  

        return effectiveU235Density * FissionCrossSection * BaseNeutronFlux;
    }

    public float GetNeutronProduction()
    {
        float totalNeutrons = 0f;
        foreach (FuelRod rod in fuelRods)
        {
            totalNeutrons += rod.neutronProduction;
        }

        float absorbedNeutrons = controlRodManager.GetTotalNeutronAbsorption(totalNeutrons);
        float netNeutrons = Mathf.Max(0, totalNeutrons - absorbedNeutrons); // Evitar valores negativos

        return netNeutrons;
    }
}
