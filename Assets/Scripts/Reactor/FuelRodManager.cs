using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.UI;
using System.Globalization;
using TMPro;

public class FuelRodManager : MonoBehaviour
{

    public TMP_Text neutronFluxText;
    public TMP_Text temperatureText;
    public TMP_Text productionText;
    public TMP_Text lostText;
    public TMP_Text cambioText;
    public TMP_Text kEffText;

    private NativeArray<float> neutronProductions;
    private NativeArray<float> neutronLosses;


    // Configuración modificada
    [Header("Prefab Configuration")]
    public GameObject fuelRodPrefab;
    public Material fuelRodMaterial;

    [Header("Core Geometry")]
    public int gridSize = 6;
    public float rodSpacing = 0.15f; // 15 cm
    public float rodDiameter = 0.015f; // 1.5 cm
    public float rodHeight = 4.0f; // 4 metros

    [Header("Neutron Physics")]
    public float initialFlux = 1e14f; // Mayor flujo inicial
    public float maxFlux = 1e16f;
    public float neutronLifetime = 1e-4f; // Vida media más larga

    [Header("Thermal Physics")]
    public float coolantTemperature = 300f;
    public float heatTransferCoefficient = 5e4f;



    private List<FuelRod> fuelRods = new List<FuelRod>();
    private NativeArray<float> neutronFluxes;
    private NativeArray<float> temperatures;
    private NativeArray<Vector3> positions;

    void Start()
    {
        CreateFuelAssembly();
        InitializeNeutronData();
    }

    void CreateFuelAssembly()
    {
        // Destruir varillas existentes
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        fuelRods.Clear();

        float horizontalSpacing = 0.5f; // Espaciado realista (50 cm)
        float verticalSpacing = horizontalSpacing * Mathf.Sqrt(3)/2;
        int rodsPerAssembly = 6; // Varillas por "módulo"

        int rodCount = 0;
        for(int x = 0; x < gridSize; x++)
        {
            for(int z = 0; z < gridSize; z++)
            {
                // Posición hexagonal para 6 varillas por módulo
                for(int i = 0; i < rodsPerAssembly; i++)
                {
                    Vector3 position = new Vector3(
                        x * horizontalSpacing + (z % 2) * horizontalSpacing/2,
                        0.1f + i * 0.4f, // Apilamiento vertical
                        z * verticalSpacing
                    );

                    // Centrado del núcleo
                    position -= new Vector3(
                        gridSize * horizontalSpacing / 2,
                        0,
                        gridSize * verticalSpacing / 2
                    );

                    GameObject rodObj = Instantiate(fuelRodPrefab, position, Quaternion.identity, transform);
                    rodObj.transform.localScale = new Vector3(
                        0.015f, // 1.5 cm diámetro
                        4.0f,   // 4 metros altura
                        0.015f
                    );

                    // Configurar escala y componentes
                    rodObj.transform.localScale = new Vector3(2f, 30f, 2f);


                    // Configurar material
                    Renderer renderer = rodObj.GetComponent<Renderer>();
                    renderer.material = new Material(fuelRodMaterial);

                    FuelRod rod = rodObj.AddComponent<FuelRod>();
                    rod.InitializeNuclearParameters(
                        enrichment: 0.05f,
                        density: 10.97f,
                        coolantTemp: coolantTemperature,
                        heatTransfer: heatTransferCoefficient
                    );

                    fuelRods.Add(rod);

                    rodCount++;
                    if(rodCount >= 216) return; // Parar al llegar a 216
                }
            }   
        }
    }

    void InitializeNeutronData()
    {
        if (neutronFluxes.IsCreated) neutronFluxes.Dispose();
        neutronFluxes = new NativeArray<float>(fuelRods.Count, Allocator.Persistent);
        temperatures = new NativeArray<float>(fuelRods.Count, Allocator.Persistent);
        positions = new NativeArray<Vector3>(fuelRods.Count, Allocator.Persistent);
        neutronProductions = new NativeArray<float>(fuelRods.Count, Allocator.Persistent);
        neutronLosses = new NativeArray<float>(fuelRods.Count, Allocator.Persistent);


        for (int i = 0; i < fuelRods.Count; i++)
        {
            neutronFluxes[i] = initialFlux;
            temperatures[i] = coolantTemperature;
            positions[i] = fuelRods[i].transform.position;
            fuelRods[i].SetFlux(initialFlux);
        }
    }

    void Update()
    {
        if (fuelRods.Count == 0) return;

        // Ejecutar simulación paralelizada
        RunNeutronSimulation(Time.deltaTime);
        UpdateVisuals();
        UpdateUI();
    }

    void RunNeutronSimulation(float deltaTime)
    {
        var job = new NeutronSimulationJob
        {
            positions = positions,
            neutronFluxes = neutronFluxes,
            temperatures = temperatures,
            deltaTime = deltaTime,
            neutronLifetime = neutronLifetime,
            maxFlux = maxFlux,
            coolantTemp = coolantTemperature,
            rodSpacing = rodSpacing,
            neutronProductions = neutronProductions,
            neutronLosses = neutronLosses
        };

        JobHandle handle = job.Schedule(fuelRods.Count, 64);
        handle.Complete();

        // Actualizar componentes
        for (int i = 0; i < fuelRods.Count; i++)
        {
            fuelRods[i].SetFluxAndTemp(neutronFluxes[i], temperatures[i]);
            fuelRods[i].SetNeutronData(neutronProductions[i], neutronLosses[i]); // 🔴 Nuevo método
        }

    }

    void UpdateVisuals()
    {
        foreach (FuelRod rod in fuelRods)
        {
            rod.UpdateVisual();
        }
    }

    void UpdateUI()
    {
        
        if (fuelRods == null || fuelRods.Count == 0) return;
        if (fuelRods.Count > 0)
        {
            FuelRod centralRod = fuelRods[fuelRods.Count / 2];

            float totalProduction = 0f;
            float totalLoss = 0f;

            foreach (FuelRod rod in fuelRods)
            {
                totalProduction += rod.neutronProduction;
                totalLoss += rod.neutronLoss;
            }

            float kEff = (totalLoss > 0) ? totalProduction / totalLoss : 0f;

            neutronFluxText.text = $"Neutron Flux: {centralRod.currentFlux:0.##E0} n/cm²s";
            temperatureText.text = $"Temperature: {centralRod.temperature:F2} K";
            productionText.text = $"Producción: {centralRod.neutronProduction:F2}";
            lostText.text = $"Pérdida: {centralRod.neutronLoss:F2}";
            cambioText.text = $"Cambio: {centralRod.neutronProduction - centralRod.neutronLoss:F2}";
            kEffText.text = $"k_eff: {kEff:F5}";

            
            if (Mathf.Abs(kEff - 1.0f) < 0.01f)
            {
                Debug.Log("🔥 Reactor en estado CRÍTICO.");
            }
            else if (kEff > 1.0f)
            {
                Debug.Log("⚠️ Reactor en estado SUPERCRÍTICO (potencia en aumento).");
            }
            else
            {
                Debug.Log("🔵 Reactor en estado SUBCRÍTICO (potencia decayendo).");
            }
            

            //Debug.Log($"Producción Total: {totalProduction}, Pérdida Total: {totalLoss}, k_eff: {kEff}");
        }
    }

    void OnDestroy()
    {
        if (neutronFluxes.IsCreated) neutronFluxes.Dispose();
        if (temperatures.IsCreated) temperatures.Dispose();
        if (positions.IsCreated) positions.Dispose();
        if (neutronProductions.IsCreated) neutronProductions.Dispose(); // 🔴 Asegurar que se libera
        if (neutronLosses.IsCreated) neutronLosses.Dispose(); // 🔴 Asegurar que se libera
    }

    void OnDisable()
    {
        OnDestroy();
    }

    void OnDrawGizmos()
    {
        if (fuelRods == null) return; // Evita errores si la lista aún no está inicializada

        Gizmos.color = Color.red;
        foreach (var rod in fuelRods)
        {
            if (rod != null)
            {
                Gizmos.DrawWireCube(rod.transform.position, new Vector3(0.015f, 4.0f, 0.015f));
            }
        }
    }

    [BurstCompile]
    struct NeutronSimulationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> positions;
        public NativeArray<float> neutronFluxes;
        public NativeArray<float> temperatures;
        public NativeArray<float> neutronProductions;
        public NativeArray<float> neutronLosses;
        
        public float deltaTime;
        public float neutronLifetime;
        public float maxFlux;
        public float coolantTemp;
        public float rodSpacing;

        public void Execute(int index)
        {
            // 1. Factor de posición (radio del núcleo 1.5 metros)
            Vector3 pos = positions[index];
            float distance = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
            float positionFactor = Mathf.Clamp01(1 - distance/1.5f);

            // 2. Probabilidades realistas (valores para reactor PWR)
            float fissionProb = 0.85f * positionFactor;
            float absorptionProb = 0.12f * positionFactor;
            float leakageProb = 0.03f * (1 - positionFactor);

            // Efecto Doppler: Ajuste de la absorción con la temperatura
            float tempFactor = Mathf.Clamp01((temperatures[index] - coolantTemp) / 1000f);
            absorptionProb *= (1 + 0.4f * tempFactor); // Aumento de la absorción con la temperatura

            // 3. Balance neutrónico mejorado
            float currentFlux = neutronFluxes[index];
            
            // Producción por fisión (k-eff ≈ 1.0)
            float neutronProduction = currentFlux * fissionProb * 2.43f;
            
            // Pérdidas
            float neutronLoss = currentFlux * (absorptionProb + leakageProb);
            
            // Término de decaimiento (vida media 1e-4 segundos)
            float decayTerm = currentFlux * deltaTime / neutronLifetime;

            // 4. Actualizar flujo con límites físicos
            float newFlux = currentFlux + (neutronProduction - neutronLoss) * deltaTime - decayTerm;
            newFlux = Mathf.Clamp(newFlux, 1e12f, maxFlux);

            // 5. Cálculo de temperatura realista
            float powerDensity = newFlux * fissionProb * 200e6f * 1.602e-19f; // W/cm³
            float cooling = (temperatures[index] - coolantTemp) * 0.005f; // Coeficiente reducido
            float newTemp = temperatures[index] + (powerDensity - cooling) * deltaTime;

            // 6. Aplicar resultados
            neutronFluxes[index] = newFlux;
            temperatures[index] = Mathf.Clamp(newTemp, coolantTemp, 1500f);

            neutronProductions[index] = neutronProduction;
            neutronLosses[index] = neutronLoss;
        }
    }
}


//#######################################################################################################################################

/*using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.UI;

public class FuelRodManager : MonoBehaviour
{
    // Configuración modificada
    [Header("Prefab Configuration")]
    public GameObject fuelRodPrefab;
    public Material fuelRodMaterial;

    [Header("Core Geometry")]
    public int gridSize = 6;
    public float rodSpacing = 0.15f; // 15 cm
    public float rodDiameter = 0.015f; // 1.5 cm
    public float rodHeight = 4.0f; // 4 metros

    [Header("Neutron Physics")]
    public float initialFlux = 1e12f;
    public float maxFlux = 1e15f;
    // En el Header "Neutron Physics"
    public float neutronLifetime = 1e-2f;  // Cambiado de 1e-4 a 1e-2

    [Header("Thermal Physics")]
    public float coolantTemperature = 300f;
    public float heatTransferCoefficient = 5e4f;

    [Header("UI References")]
    public Text neutronFluxText;
    public Text temperatureText;

    private List<FuelRod> fuelRods = new List<FuelRod>();
    private NativeArray<float> neutronFluxes;
    private NativeArray<float> temperatures;
    private NativeArray<Vector3> positions;

    void Start()
    {
        CreateFuelAssembly();
        InitializeNeutronData();
    }

    void CreateFuelAssembly()
    {
        // Destruir varillas existentes
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        fuelRods.Clear();

        float adjustedSpacing = 4f; // Asegura que haya espacio suficiente
        float horizontalSpacing = adjustedSpacing;
        float verticalSpacing = adjustedSpacing * Mathf.Sqrt(3) / 2;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                // Posición hexagonal
                Vector3 position = new Vector3(
                    x * horizontalSpacing + (z % 2) * horizontalSpacing/2,
                    rodHeight/2,
                    z * verticalSpacing
                );

                // Centrar el núcleo
                position -= new Vector3(
                    gridSize * horizontalSpacing / 2,
                    0,
                    gridSize * verticalSpacing / 2
                );

                GameObject rodObj = Instantiate(
                    fuelRodPrefab,
                    position,
                    Quaternion.identity,
                    transform
                );

                // Configurar escala y componentes
                rodObj.transform.localScale = new Vector3(2f, 30f, 2f);


                // Configurar material
                Renderer renderer = rodObj.GetComponent<Renderer>();
                renderer.material = new Material(fuelRodMaterial);

                FuelRod rod = rodObj.AddComponent<FuelRod>();
                rod.InitializeNuclearParameters(
                    enrichment: 0.05f,
                    density: 10.97f,
                    coolantTemp: coolantTemperature,
                    heatTransfer: heatTransferCoefficient
                );

                fuelRods.Add(rod);
            }
        }
    }

    void InitializeNeutronData()
    {
        neutronFluxes = new NativeArray<float>(fuelRods.Count, Allocator.Persistent);
        temperatures = new NativeArray<float>(fuelRods.Count, Allocator.Persistent);
        positions = new NativeArray<Vector3>(fuelRods.Count, Allocator.Persistent);

        for (int i = 0; i < fuelRods.Count; i++)
        {
            neutronFluxes[i] = initialFlux;
            temperatures[i] = coolantTemperature;
            positions[i] = fuelRods[i].transform.position;
            fuelRods[i].SetFlux(initialFlux);
        }
    }

    void Update()
    {
        if (fuelRods.Count == 0) return;

        // Ejecutar simulación paralelizada
        RunNeutronSimulation(Time.deltaTime);
        UpdateVisuals();
        UpdateUI();
    }

    void RunNeutronSimulation(float deltaTime)
    {
        var job = new NeutronSimulationJob
        {
            positions = positions,
            neutronFluxes = neutronFluxes,
            temperatures = temperatures,
            deltaTime = deltaTime,
            neutronLifetime = neutronLifetime,
            maxFlux = maxFlux,
            coolantTemp = coolantTemperature,
            rodSpacing = rodSpacing
        };

        JobHandle handle = job.Schedule(fuelRods.Count, 64);
        handle.Complete();

        // Actualizar componentes
        for (int i = 0; i < fuelRods.Count; i++)
        {
            fuelRods[i].SetFluxAndTemp(neutronFluxes[i], temperatures[i]);
        }
    }

    void UpdateVisuals()
    {
        foreach (FuelRod rod in fuelRods)
        {
            rod.UpdateVisual();
        }
    }

    void UpdateUI()
    {
        if (fuelRods.Count > 0)
        {
            FuelRod centralRod = fuelRods[fuelRods.Count/2];
            neutronFluxText.text = $"Neutron Flux: {centralRod.currentFlux.ToString("E2")} n/cm²s";
            temperatureText.text = $"Temperature: {centralRod.temperature.ToString("F0")} K";
        }
    }

    void OnDestroy()
    {
        if (neutronFluxes.IsCreated) neutronFluxes.Dispose();
        if (temperatures.IsCreated) temperatures.Dispose();
        if (positions.IsCreated) positions.Dispose();
    }

    [BurstCompile]
    struct NeutronSimulationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> positions;
        public NativeArray<float> neutronFluxes;
        public NativeArray<float> temperatures;
        
        public float deltaTime;
        public float neutronLifetime;
        public float maxFlux;
        public float coolantTemp;
        public float rodSpacing;

        public void Execute(int index)
        {
            // 1. Factor de posición radial
            Vector3 pos = positions[index];
            float coreRadius = rodSpacing * 6 / 2;
            float distance = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
            float positionFactor = Mathf.Exp(-distance / coreRadius);

            // 2. Cálculo de reactividad
            float fissionProb = 1.2f * positionFactor;  // Aumentado de 0.85 a 1.2 para mayor reactividad
            float absorptionProb = 0.15f * positionFactor; // Absorción
            float leakageProb = 0.05f * (1 - positionFactor); // Fugas

            // 3. Balance neutrónico
            float currentFlux = neutronFluxes[index];
            float neutronProduction = currentFlux * fissionProb * 2.43f;
            float neutronLoss = currentFlux * (absorptionProb + leakageProb);
            
            // 4. Actualizar flujo con decaimiento correcto
            float decayTerm = currentFlux / neutronLifetime;
            float newFlux = currentFlux + (neutronProduction - neutronLoss - decayTerm) * deltaTime;
            newFlux = Mathf.Clamp(newFlux, 1e10f, maxFlux);  // Eliminar el Mathf.Exp

            // 5. Calcular temperatura (ajustar parámetros si es necesario)
            float powerDensity = newFlux * fissionProb * 200e6f * 1.602e-19f;
            float cooling = (temperatures[index] - coolantTemp) * 0.02f;
            float newTemp = temperatures[index] + (powerDensity - cooling) * deltaTime;

            // 6. Aplicar resultados
            neutronFluxes[index] = newFlux;
            temperatures[index] = Mathf.Clamp(newTemp, coolantTemp, 2000f);
        }
    }
}
*/