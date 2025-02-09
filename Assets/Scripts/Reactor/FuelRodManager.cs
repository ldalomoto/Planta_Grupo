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

    public control controlSystem;


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



    public List<FuelRod> fuelRods = new List<FuelRod>();
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

                    rodObj.SetActive(false);

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

        float controlRodAbsorption = 0f;
        foreach (var rod in controlSystem.controlRods)
        {
            if (rod.isInserted)
            {
                controlRodAbsorption += (rod.neutronAbsorptionEfficiency);
                //Debug.Log($"Barra absorbente {rod.rodID}: {rod.neutronAbsorptionEfficiency}");
            }
        }

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
            neutronLosses = neutronLosses,
            //controlRodAbsorption = Mathf.Clamp01(controlRodAbsorption)
            controlRodAbsorption = controlRodAbsorption
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

    /*
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
    */

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

        public float controlRodAbsorption;

        public void Execute(int index)
        {
            // 1. Factor de posición
            Vector3 pos = positions[index];
            float distance = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
            float positionFactor = Mathf.Clamp(1 - distance/1.5f, 0f, 1f);

            // 2. Probabilidades base
            float fissionProb = 0.85f * positionFactor;
            float absorptionProb = 0.12f * positionFactor;
            float leakageProb = 0.03f * (1 - positionFactor);

            // 3. Efecto Doppler
            float dopplerEffect = Mathf.Clamp((temperatures[index] - 300f) * -2.5e-5f, -0.2f, 0f);
            absorptionProb *= (1 + dopplerEffect);

            // 4. Barras de control (efecto espacial real)
            float controlEffect = controlRodAbsorption * Mathf.Exp(-distance/0.5f); // Decaimiento radial
            absorptionProb += controlEffect * 0.2f;

            // 5. Balance neutrónico
            float currentFlux = neutronFluxes[index];
            float neutronProduction = currentFlux * fissionProb * 2.43f;
            float neutronLoss = currentFlux * (absorptionProb + leakageProb);

            // 6. Actualizar flujo
            float newFlux = currentFlux + (neutronProduction - neutronLoss) * deltaTime;
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

/*using UnityEngine;
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

    public control controlSystem;


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
    public float initialFlux = 1e12f; // Mayor flujo inicial
    public float maxFlux = 1e15f;
    public float neutronLifetime = 1e-5f; // Vida media más larga

    [Header("Thermal Physics")]
    public float coolantTemperature = 300f;
    public float heatTransferCoefficient = 5e4f;

    public static readonly float fuelSpecificHeat = 300f;  // J/kg·K (UO2 real)



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

        float controlRodAbsorption = 0f;
        foreach (var rod in controlSystem.controlRods)
        {
            if (rod.isInserted)
            {
                controlRodAbsorption += (rod.neutronAbsorptionEfficiency);
                //Debug.Log($"Barra absorbente {rod.rodID}: {rod.neutronAbsorptionEfficiency}");
            }
        }

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
            neutronLosses = neutronLosses,
            //controlRodAbsorption = Mathf.Clamp01(controlRodAbsorption)
            controlRodAbsorption = controlRodAbsorption
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

        public float controlRodAbsorption;

        float BesselJ0(float x)
        {
            float ax = Mathf.Abs(x);
            float y = ax * ax;

            float ans1 = 1.0f - y * (2.2499997e-1f - y * (1.2656208e-2f - y * (3.167633e-4f - y * (3.843164e-6f - y * 1.048843e-8f))));
            return ans1;
        }

        public void Execute(int index)
        {
            // 1. Factor de posición
            Vector3 pos = positions[index];
            float distance = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
            float positionFactor = BesselJ0(2.405f * distance / 1.5f);  // Función de Bessel

            // 2. Probabilidades base
            float fissionProb = 0.06f * positionFactor;    // Valor realista
            float absorptionProb = 0.12f * positionFactor;
            float leakageProb = 0.4f * (distance / 1.5f);  // Fugas aumentadas
            
            // 3. Efecto Doppler
            float dopplerEffect = Mathf.Clamp((temperatures[index] - 600f) * -5e-5f, -0.3f, 0f);
            absorptionProb *= (1 + dopplerEffect);

            // 4. Barras de control (efecto espacial real)
            float controlEffect = controlRodAbsorption * Mathf.Exp(-Mathf.Pow(distance/0.3f, 2)); 
            absorptionProb += controlEffect * 8.65f;  // Eficiencia calibrada para Cd

            // 5. Balance neutrónico
            float currentFlux = neutronFluxes[index];
            float neutronProduction = currentFlux * fissionProb * 2.43f;
            float neutronLoss = currentFlux * (absorptionProb + leakageProb);
            
            // 6. Decaimiento exponencial si reactor subcrítico
            if(controlRodAbsorption > 0.5f && neutronLoss > neutronProduction){
                float decayTime = 80f;  // Vida media de decaimiento realista
                currentFlux *= Mathf.Exp(-deltaTime / decayTime);
            }

            float newFlux = Mathf.Clamp(currentFlux + (neutronProduction - neutronLoss) * deltaTime, 1e10f, maxFlux);

            // 7. Cálculo térmico realista con inercia
            float powerDensity = newFlux * fissionProb * 200e6f * 1.602e-19f; // W/cm³
            float cooling = (temperatures[index] - coolantTemp) * 0.05f;       // Coeficiente aumentado
            
            // Masa de combustible realista (UO2)
            float mass = 10.97f * Mathf.PI * Mathf.Pow(0.0075f, 2) * 400f; // kg (4m altura)
            float newTemp = temperatures[index] + (powerDensity - cooling) * deltaTime / (mass * fuelSpecificHeat);

            // 8. Aplicar resultados
            neutronFluxes[index] = newFlux;
            temperatures[index] = Mathf.Clamp(newTemp, coolantTemp, 1500f);

            neutronProductions[index] = neutronProduction;
            neutronLosses[index] = neutronLoss;
        }
    }
}





*/


/*using UnityEngine;
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

    public control controlSystem;


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

    // Agregar nuevos arrays para control
    private NativeArray<Vector3> controlRodPositions;
    private NativeArray<bool> controlRodInsertions;
    private NativeArray<float> controlRodEfficiencies;

    void Start()
    {
        CreateFuelAssembly();
        InitializeNeutronData();
        InitializeControlRodData(); // Nuevo método inicializador
    }


    void InitializeControlRodData()
    {
        controlRodPositions = new NativeArray<Vector3>(controlSystem.controlRods.Count, Allocator.Persistent);
        controlRodInsertions = new NativeArray<bool>(controlSystem.controlRods.Count, Allocator.Persistent);
        controlRodEfficiencies = new NativeArray<float>(controlSystem.controlRods.Count, Allocator.Persistent);
        UpdateControlRodData();
    }


    void UpdateControlRodData()
    {
        for(int i = 0; i < controlSystem.controlRods.Count; i++)
        {
            controlRodPositions[i] = controlSystem.controlRods[i].rodTransform.position;
            controlRodInsertions[i] = controlSystem.controlRods[i].isInserted;
            controlRodEfficiencies[i] = controlSystem.controlRods[i].neutronAbsorptionEfficiency;
        }
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

        UpdateControlRodData(); // Actualizar datos cada frame

        float controlRodAbsorption = 0f;
        foreach (var rod in controlSystem.controlRods)
        {
            if (rod.isInserted)
            {
                controlRodAbsorption += (rod.neutronAbsorptionEfficiency);
                //Debug.Log($"Barra absorbente {rod.rodID}: {rod.neutronAbsorptionEfficiency}");
            }
        }

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
            neutronLosses = neutronLosses,
            //controlRodAbsorption = Mathf.Clamp01(controlRodAbsorption)
            controlRodAbsorption = controlRodAbsorption,

            controlRodPositions = controlRodPositions,
            controlRodInsertions = controlRodInsertions,
            controlRodEfficiencies = controlRodEfficiencies,
            rodEffectRadius = rodEffectRadius
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

        // Liberar los nuevos arrays
        if(controlRodPositions.IsCreated) controlRodPositions.Dispose();
        if(controlRodInsertions.IsCreated) controlRodInsertions.Dispose();
        if(controlRodEfficiencies.IsCreated) controlRodEfficiencies.Dispose();

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

        [ReadOnly] public NativeArray<Vector3> controlRodPositions;
        [ReadOnly] public NativeArray<bool> controlRodInsertions;
        [ReadOnly] public NativeArray<float> controlRodEfficiencies;
        public float rodEffectRadius;
        
        public float deltaTime;
        public float neutronLifetime;
        public float maxFlux;
        public float coolantTemp;
        public float rodSpacing;

        public float controlRodAbsorption;

        public void Execute(int index)
        {
            // 1. Factor de posición
            Vector3 pos = positions[index];
            float distance = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
            float positionFactor = Mathf.Clamp(1 - distance/1.5f, 0f, 1f);

            // 2. Probabilidades base
            float fissionProb = 0.85f * positionFactor;
            float absorptionProb = 0.12f * positionFactor;
            float leakageProb = 0.03f * (1 - positionFactor);

            // 3. Efecto Doppler
            float dopplerEffect = Mathf.Clamp((temperatures[index] - 300f) * -2.5e-5f, -0.2f, 0f);
            absorptionProb *= (1 + dopplerEffect);

            // 4. Barras de control (efecto espacial real)
            float controlEffect = 0f;
            for(int i = 0; i < controlRodPositions.Length; i++)
            {
                if(!controlRodInsertions[i]) continue;
                
                float distance = Vector3.Distance(pos, controlRodPositions[i]);
                if(distance < rodEffectRadius)
                {
                    float effect = controlRodEfficiencies[i] * Mathf.Exp(-distance/(rodEffectRadius * 0.5f));
                    controlEffect += effect;
                }
            }
            absorptionProb += controlEffect;

            // 5. Balance neutrónico
            float currentFlux = neutronFluxes[index];
            float neutronProduction = currentFlux * fissionProb * 2.43f;
            float neutronLoss = currentFlux * (absorptionProb + leakageProb);

            // 6. Actualizar flujo
            float newFlux = currentFlux + (neutronProduction - neutronLoss) * deltaTime;
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
*/


//#######################################################################################################################################

/*using UnityEngine;
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

    public control controlSystem;


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

        float controlRodAbsorption = 0f;
        foreach (var rod in controlSystem.controlRods)
        {
            if (rod.isInserted)
            {
                controlRodAbsorption += (rod.neutronAbsorptionEfficiency);
                //Debug.Log($"Barra absorbente {rod.rodID}: {rod.neutronAbsorptionEfficiency}");
            }
        }

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
            neutronLosses = neutronLosses,
            //controlRodAbsorption = Mathf.Clamp01(controlRodAbsorption)
            controlRodAbsorption = controlRodAbsorption
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

        public float controlRodAbsorption;

        public void Execute(int index)
        {
            // 1. Factor de posición
            Vector3 pos = positions[index];
            float distance = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
            float positionFactor = Mathf.Clamp(1 - distance/1.5f, 0f, 1f);

            // 2. Probabilidades base
            float fissionProb = 0.85f * positionFactor;
            float absorptionProb = 0.12f * positionFactor;
            float leakageProb = 0.03f * (1 - positionFactor);

            // 3. Efecto Doppler
            float dopplerEffect = Mathf.Clamp((temperatures[index] - 300f) * -2.5e-5f, -0.2f, 0f);
            absorptionProb *= (1 + dopplerEffect);

            // 4. Barras de control (efecto espacial real)
            float controlEffect = controlRodAbsorption * Mathf.Exp(-distance/0.5f); // Decaimiento radial
            absorptionProb += controlEffect * 0.2f;

            // 5. Balance neutrónico
            float currentFlux = neutronFluxes[index];
            float neutronProduction = currentFlux * fissionProb * 2.43f;
            float neutronLoss = currentFlux * (absorptionProb + leakageProb);

            // 6. Actualizar flujo
            float newFlux = currentFlux + (neutronProduction - neutronLoss) * deltaTime;
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
*/