using UnityEngine;

public class FuelRod
{
    public int X;
    public int Y;
    public float enrichment = 0.05f; // 5% U-235
    public float density = 10.97f; // g/cm³ (UO2)
    public float temperature = 300.0f; // Kelvin
    public float burnup = 0.0f; // MWd/kgU
    public float neutronFlux = 1e14f; // n/cm²s inicial
    public float neutronProduction;
    public float neutronAbsorption;
    private const float sigmaFission = 0.0585f; // Sección eficaz de fisión (barns)
    private const float sigmaAbsorption = 0.07f; // Sección eficaz de absorción (barns)
    private const float neutronsPerFission = 2.6f; // Neutrones por fisión
    private const float diffusionCoefficient = 0.003f; // Factor de difusión de neutrones
    private float U235Density;

    public FuelRod(int x, int y)
    {
        this.X = x;
        this.Y = y;
        U235Density = density * enrichment; // Calcular la densidad de U-235
    }

    public void UpdateNeutronFlux(FuelRod[,] fuelRods, int gridSize)
    {
        float fissionRate = neutronFlux * sigmaFission * U235Density;
        float absorptionRate = neutronFlux * sigmaAbsorption;
        float diffusionEffect = 0.0f;

        // Difusión de neutrones entre celdas vecinas
        if (X > 0) diffusionEffect += diffusionCoefficient * (fuelRods[X - 1, Y].neutronFlux - neutronFlux);
        if (X < gridSize - 1) diffusionEffect += diffusionCoefficient * (fuelRods[X + 1, Y].neutronFlux - neutronFlux);
        if (Y > 0) diffusionEffect += diffusionCoefficient * (fuelRods[X, Y - 1].neutronFlux - neutronFlux);
        if (Y < gridSize - 1) diffusionEffect += diffusionCoefficient * (fuelRods[X, Y + 1].neutronFlux - neutronFlux);

        neutronProduction = fissionRate * neutronsPerFission;
        neutronAbsorption = absorptionRate;
        neutronFlux += neutronProduction - neutronAbsorption + diffusionEffect;
    }

    public void CalculateTemperature(float coolantTemp, float deltaTime)
    {
        float powerDensity = neutronFlux * sigmaFission * 200e6f; // 200 MeV por fisión
        temperature += (powerDensity - (temperature - coolantTemp)) * deltaTime;
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

