using UnityEngine;

public class FuelRod : MonoBehaviour
{
    [Header("Nuclear Parameters")]
    public float enrichment = 0.05f;
    public float density = 10.97f;
    
    [Header("Current Status")]
    public float currentFlux;
    public float temperature;

    // Parámetros privados
    private float u235Atoms;
    private float coolantTemperature;
    private float heatTransferCoefficient;
    private Renderer rodRenderer;

    public float neutronProduction;
    public float neutronLoss;

    public void SetNeutronData(float production, float loss)
    {
        neutronProduction = production;
        neutronLoss = loss;
    }

    public void InitializeNuclearParameters(float enrichment, float density, float coolantTemp, float heatTransfer)
    {
        this.enrichment = enrichment;
        this.density = density;
        this.coolantTemperature = coolantTemp;
        this.heatTransferCoefficient = heatTransfer;
        
        rodRenderer = GetComponent<Renderer>();
        CalculateU235Density();
    }

    void CalculateU235Density()
    {
        float molarMassUO2 = (235f * enrichment) + (238f * (1 - enrichment)) + (2 * 16f);
        u235Atoms = (density * enrichment * 6.022e23f) / molarMassUO2;
    }

    public void SetFluxAndTemp(float flux, float temp)
    {
        currentFlux = flux;
        temperature = temp;
    }

    public void SetFlux(float flux)
    {
        currentFlux = flux;
    }

    public void UpdateVisual()
    {
        // Mapear temperatura a color (300K-1500K)
        float tempRatio = Mathf.InverseLerp(300f, 1500f, temperature);
        Color rodColor = Color.Lerp(
            new Color(0, 0.3f, 1f),    // Azul frío
            new Color(1f, 0.2f, 0f),   // Rojo caliente
            tempRatio
        );
        
        rodRenderer.material.color = rodColor;
        rodRenderer.material.SetColor("_EmissionColor", rodColor * 0.3f);
    }
}
/*
using UnityEngine;

public class FuelRod : MonoBehaviour
{
    // **Constantes físicas**
    private const float AvogadroNumber = 6.022e23f; // Número de Avogadro
    private const float U235AtomicMass = 235.04f; // g/mol
    private const float U238AtomicMass = 238.05f; // g/mol
    private const float UraniumDioxideDensity = 10.97f; // g/cm³
    private const float MeVToJoule = 1.60218e-13f; // Conversión MeV -> Joules

    // **Propiedades del combustible**
    public float uraniumEnrichment = 0.05f; // 5% de U-235
    public float rodLength = 4.0f; // m
    public float rodDiameter = 0.015f; // m
    public int pelletsPerRod = 80; // Número de pastillas
    public float pelletDiameter = 0.01f; // m
    public float pelletHeight = 0.05f; // m
    public float fissionEnergy = 200e6f; // Energía liberada por fisión (MeV)

    // **Propiedades calculadas**
    private float rodVolume;
    private float fuelMass;
    private float u235Mass;
    private float u238Mass;
    private float availableFissions;

    // **Temperaturas**
    public float surfaceTemperature = 300f; // °C
    public float coreTemperature = 1200f; // °C
    private float uraniumMeltingPoint = 2800f; // °C

    void Start()
    {
        CalculateFuelProperties();
    }

    void CalculateFuelProperties()
    {
        // Volumen total del combustible (cilindro) en m³
        float pelletVolume = Mathf.PI * Mathf.Pow(pelletDiameter / 2, 2) * pelletHeight;
        rodVolume = pelletVolume * pelletsPerRod;

        // Masa total del combustible (densidad * volumen), en gramos
        fuelMass = UraniumDioxideDensity * rodVolume * 1e6f; // Convertir m³ a cm³

        // Masa de U-235 y U-238 en la varilla (según enriquecimiento)
        u235Mass = fuelMass * uraniumEnrichment;
        u238Mass = fuelMass * (1 - uraniumEnrichment);

        // Número de átomos de U-235 y U-238 en la varilla
        float u235Atoms = (u235Mass / U235AtomicMass) * AvogadroNumber;
        float u238Atoms = (u238Mass / U238AtomicMass) * AvogadroNumber;

        // Número total de fisiones posibles (asumiendo que solo el U-235 fisiona)
        availableFissions = u235Atoms;

        Debug.Log($"Masa total de combustible: {fuelMass} g");
        Debug.Log($"Masa de U-235: {u235Mass} g");
        Debug.Log($"Número de fisiones disponibles: {availableFissions:E2}");
    }

    public void AbsorbNeutron(float neutronFlux)
    {
        // Calcula cuántos U-235 fisionan según la sección eficaz
        float fissionCrossSection = 585f * 1e-24f; // Conversión de barns a cm²
        float fissions = neutronFlux * fissionCrossSection * availableFissions;

        // Energía generada en Joules
        float energyProduced = fissions * fissionEnergy * MeVToJoule;

        // Aumentar la temperatura con la energía generada
        IncreaseTemperature(energyProduced / 1e6f);
    }

    public void IncreaseTemperature(float heatInput)
    {
        float specificHeatUO2 = 0.3f; // J/g·K
        float temperatureIncrease = heatInput / (fuelMass * specificHeatUO2);
        coreTemperature += temperatureIncrease;
        surfaceTemperature += temperatureIncrease / 10; // Aproximación

        if (coreTemperature > uraniumMeltingPoint)
        {
            Debug.LogWarning("¡Peligro! El núcleo de la varilla ha alcanzado la temperatura de fusión.");
        }
    }

    void Update()
    {
        IncreaseTemperature(Time.deltaTime * 10);
    }
}


*/

