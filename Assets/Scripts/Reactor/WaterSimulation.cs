using UnityEngine;

public class WaterSimulation : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    public float waterLevel = 5f;  
    public float maxWaterLevel = 10f; 
    public float waveHeight = 0.05f;
    public float waveSpeed = 2.0f;

    // Control térmico
    [Header("Thermal Effects")]
    public Gradient thermalColorGradient;
    public float maxTemperature = 1500f;
    public float temperatureEffectIntensity = 0.5f;
    private Material waterMaterial;
    private FuelRodManager fuelRodManager;

    private float originalScaleY; // Almacena la escala original en Y de la cápsula

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("No se encontró MeshFilter en el objeto de agua.");
            return;
        }

        mesh = meshFilter.mesh;
        mesh.MarkDynamic();
        if (mesh == null)
        {
            Debug.LogError("No se encontró la malla en el MeshFilter.");
            return;
        }

        originalVertices = mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i];
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Inicializar material del agua
        waterMaterial = GetComponent<Renderer>().material;
        fuelRodManager = FindFirstObjectByType<FuelRodManager>();
        
        waterMaterial = Material.Instantiate(waterMaterial);
        GetComponent<Renderer>().material = waterMaterial;

        // Guardar escala original
        originalScaleY = transform.localScale.y;

        // Crear el gradiente de temperatura
        thermalColorGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[6];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[6];

        colorKeys[0] = new GradientColorKey(new Color(0.7f, 0.9f, 1f), 0f); 
        colorKeys[1] = new GradientColorKey(new Color(0.1f, 0.5f, 0.8f), 0.2f); 
        colorKeys[2] = new GradientColorKey(new Color(0f, 0.7f, 0.75f), 0.4f); 
        colorKeys[3] = new GradientColorKey(new Color(1f, 0.84f, 0.3f), 0.6f);
        colorKeys[4] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.8f); 
        colorKeys[5] = new GradientColorKey(new Color(0.85f, 0.26f, 0.08f), 1f); 

        alphaKeys[0] = new GradientAlphaKey(0.8f, 0f);
        alphaKeys[1] = new GradientAlphaKey(0.7f, 0.2f);
        alphaKeys[2] = new GradientAlphaKey(0.6f, 0.4f);
        alphaKeys[3] = new GradientAlphaKey(0.5f, 0.6f);
        alphaKeys[4] = new GradientAlphaKey(0.4f, 0.8f);
        alphaKeys[5] = new GradientAlphaKey(0.3f, 1f);

        thermalColorGradient.SetKeys(colorKeys, alphaKeys);
    }

    void Update()
    {
        if (mesh == null) return;

        SetWaterLevel(waterLevel);
        SimulateWaves();
        UpdateThermalEffects();

        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void UpdateThermalEffects()
    {
        if (fuelRodManager == null || fuelRodManager.fuelRods.Count == 0) return;

        float currentTemp = fuelRodManager.fuelRods[fuelRodManager.fuelRods.Count / 2].temperature;
        float thermalT = Mathf.Clamp01(currentTemp / maxTemperature);

        Color coolWater = new Color(0.7f, 0.9f, 1f); 
        Color hotWater = new Color(1f, 0.4f, 0.4f);  
        Color waterColor = Color.Lerp(coolWater, hotWater, thermalT);

        if (waterMaterial != null)
        {
            waterMaterial.SetColor("_BaseColor", waterColor);
            //float alpha = Mathf.Lerp(0.9f, 0.3f, thermalT);
            float alpha = Mathf.Lerp(0.9f, 0.9f, Mathf.Pow(thermalT, 0.5f)); // Atenúa el efecto térmico

            waterColor.a = alpha;
            //waterColor.a = 0.05f; // Siempre muy transparente

            //Color transparentColor = new Color(0.7f, 0.9f, 1f, 0.05f);  // Último valor define transparencia (0.05 casi invisible)
            //waterMaterial.SetColor("_BaseColor", transparentColor);

            waterMaterial.SetColor("_BaseColor", waterColor);

            Color emissionColor = Color.Lerp(Color.black, waterColor, thermalT * 0.6f);
            waterMaterial.SetColor("_EmissionColor", emissionColor);

            waveHeight = Mathf.Lerp(0.05f, 0.2f, thermalT);
            waveSpeed = Mathf.Lerp(2f, 5f, thermalT);

            //waterMaterial.SetFloat("_Smoothness", Mathf.Lerp(0.9f, 0.5f, thermalT));
            waterMaterial.SetFloat("_Smoothness", Mathf.Lerp(0.9f, 0.8f, thermalT));
        }

        // **Reducir el tamaño de la cápsula conforme crecen las olas**
        float waterScaleFactor = Mathf.Lerp(1f, 0.87f, thermalT); 
        transform.localScale = new Vector3(transform.localScale.x, originalScaleY * waterScaleFactor, transform.localScale.z);
    }

    public void SetWaterLevel(float newLevel)
    {
        waterLevel = Mathf.Clamp(newLevel, 0f, maxWaterLevel);

        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i];
            modifiedVertices[i].y = originalVertices[i].y + waterLevel; 
        }
    }

    void SimulateWaves()
    {
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(originalVertices[i]);
            modifiedVertices[i].y += Mathf.Sin(Time.time * waveSpeed + worldPos.x + worldPos.z) * waveHeight;
        }
    }

    public void ChangeWaterLevel(float delta)
    {
        waterLevel += delta;
    }
}




/*using UnityEngine;

public class WaterSimulation : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    public float waterLevel = 5f;  // Altura inicial del agua
    public float maxWaterLevel = 10f; // Altura máxima del agua
    public float waveHeight = 0.05f; // Altura de las olas
    public float waveSpeed = 2.0f; // Velocidad de las olas

    // Nuevas variables para control térmico
    [Header("Thermal Effects")]
    public Gradient thermalColorGradient;
    public float maxTemperature = 1500f;
    public float temperatureEffectIntensity = 0.5f;
    private Material waterMaterial;
    private FuelRodManager fuelRodManager;

    void Start()
    {
        // Asegurar que el objeto tiene un MeshFilter y un MeshRenderer
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("No se encontró MeshFilter en el objeto de agua.");
            return;
        }

        mesh = meshFilter.mesh;
        mesh.MarkDynamic();
        if (mesh == null)
        {
            Debug.LogError("No se encontró la malla en el MeshFilter.");
            return;
        }

        originalVertices = mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i];
        }

        // Asegurar que la malla tenga normales correctas
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Inicializar sistema térmico
        waterMaterial = GetComponent<Renderer>().material;
        fuelRodManager = FindObjectOfType<FuelRodManager>();
        
        // Crear instancia única del material
        waterMaterial = Material.Instantiate(waterMaterial);
        GetComponent<Renderer>().material = waterMaterial;
    }

    void Update()
    {
        if (mesh == null) return;

        SetWaterLevel(waterLevel);
        SimulateWaves();

        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();  // Asegurar que la malla tiene límites correctos
    }

    void UpdateThermalEffects()
    {
        if (fuelRodManager == null || fuelRodManager.fuelRods.Count == 0) return;

        // Obtener temperatura de la varilla central
        float currentTemp = fuelRodManager.fuelRods[fuelRodManager.fuelRods.Count / 2].temperature;
        
        // Calcular efectos térmicos
        float thermalT = Mathf.Clamp01(currentTemp / maxTemperature);
        Color waterColor = thermalColorGradient.Evaluate(thermalT);
        
        // Aplicar efectos al material HDRP
        if (waterMaterial != null)
        {
            // Configurar color y transparencia
            waterColor.a = Mathf.Lerp(0.8f, 0.3f, thermalT); // Reduce transparencia con temperatura
            waterMaterial.SetColor("_BaseColor", waterColor);
            
            // Añadir efecto de "ebullición" a las olas
            waveHeight = Mathf.Lerp(0.05f, 0.2f, thermalT);
            waveSpeed = Mathf.Lerp(2f, 5f, thermalT);
            
            // Ajustar propiedades HDRP adicionales
            waterMaterial.SetFloat("_Smoothness", Mathf.Lerp(0.8f, 0.4f, thermalT));
        }
    }

    public void SetWaterLevel(float newLevel)
    {
        waterLevel = Mathf.Clamp(newLevel, 0f, maxWaterLevel);

        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i]; // Restauramos los valores originales
            modifiedVertices[i].y = originalVertices[i].y + waterLevel; 
        }
    }

    void SimulateWaves() {
        for (int i = 0; i < modifiedVertices.Length; i++) {
            Vector3 worldPos = transform.TransformPoint(originalVertices[i]);
            modifiedVertices[i].y += Mathf.Sin(Time.time * waveSpeed + worldPos.x + worldPos.z) * waveHeight;
        }
    }

    public void ChangeWaterLevel(float delta)
    {
        waterLevel += delta;
    }
}
*/



//###########################################################



/*using UnityEngine;

public class WaterSimulation : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    public float waterLevel = 5f;  // Altura inicial del agua
    public float maxWaterLevel = 10f; // Altura máxima del agua
    public float waveHeight = 0.05f; // Altura de las olas
    public float waveSpeed = 2.0f; // Velocidad de las olas

    void Start()
    {
        // Asegurar que el objeto tiene un MeshFilter y un MeshRenderer
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("No se encontró MeshFilter en el objeto de agua.");
            return;
        }

        mesh = meshFilter.mesh;
        mesh.MarkDynamic();
        if (mesh == null)
        {
            Debug.LogError("No se encontró la malla en el MeshFilter.");
            return;
        }

        originalVertices = mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i];
        }

        // Asegurar que la malla tenga normales correctas
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void Update()
    {
        if (mesh == null) return;

        SetWaterLevel(waterLevel);
        SimulateWaves();

        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();  // Asegurar que la malla tiene límites correctos
    }

    public void SetWaterLevel(float newLevel)
    {
        waterLevel = Mathf.Clamp(newLevel, 0f, maxWaterLevel);

        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i]; // Restauramos los valores originales
            modifiedVertices[i].y = originalVertices[i].y + waterLevel; 
        }
    }

    void SimulateWaves() {
        for (int i = 0; i < modifiedVertices.Length; i++) {
            Vector3 worldPos = transform.TransformPoint(originalVertices[i]);
            modifiedVertices[i].y += Mathf.Sin(Time.time * waveSpeed + worldPos.x + worldPos.z) * waveHeight;
        }
    }

    public void ChangeWaterLevel(float delta)
    {
        waterLevel += delta;
    }
}
*/