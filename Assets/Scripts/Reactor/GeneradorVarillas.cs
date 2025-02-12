using UnityEngine;

public class GeneradorVarillas : MonoBehaviour
{
    public GameObject varillaPrefab; // Prefab de la varilla
    public int totalVarillas = 216;
    private float radioNucleo;
    private float alturaNucleo;
    //private float temperatura = 300f; // Temperatura inicial del refrigerante
    //private float incrementoTemperatura = 10f; // Incremento de temperatura por segundo
    
    private FuelRodManager fuelRodManager;

    void Start()
    {
        fuelRodManager = FindFirstObjectByType<FuelRodManager>();

        // Obtener el tamaño del cilindro automáticamente
        radioNucleo = GetComponent<Collider>().bounds.extents.x - 5; // Radio basado en el tamaño del cilindro
        alturaNucleo = GetComponent<Collider>().bounds.size.y; // Altura total del cilindro
        GenerarVarillas();
        //InvokeRepeating("ActualizarTemperatura", 1f, 1f); // Simulación del aumento de temperatura
    }

    void Update()
    {
        ActualizarTemperatura(); // Actualizar cada frame
    }

    void GenerarVarillas()
    {
        int filas = Mathf.CeilToInt(Mathf.Sqrt(totalVarillas)); // Determinar el número de filas
        int columnas = totalVarillas / filas;
        float espacioX = (radioNucleo * 2) / columnas; // Espaciado en X basado en el radio
        float espacioZ = (radioNucleo * 2) / filas;    // Espaciado en Z basado en el radio
        
        Vector3 escalaPrefab = varillaPrefab.transform.localScale; // Obtener la escala original del prefab
        Vector3 nuevaEscala = new Vector3(0.01f, 0.85f, 0.01f); // Ajuste manual basado en el prefab en Unity
        float desplazamientoY = nuevaEscala.y + 50f; // Bajar las varillas la misma distancia que su altura
        
        int varillasColocadas = 0;
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                if (varillasColocadas >= totalVarillas) return;
                
                float x = -radioNucleo + (j * espacioX) + (espacioX / 2);
                float z = -radioNucleo + (i * espacioZ) + (espacioZ / 2);
                
                // Verificar que la varilla está dentro del círculo
                if (x * x + z * z <= radioNucleo * radioNucleo)
                {
                    float y = transform.position.y - desplazamientoY; // Bajar las varillas
                    Vector3 posicion = new Vector3(x, y, z) + transform.position;
                    
                    GameObject varilla = Instantiate(varillaPrefab, posicion, Quaternion.identity, transform);
                    varilla.transform.localScale = nuevaEscala; // Aplicar la escala corregida
                    varilla.AddComponent<VarillaCalor>(); // Agregar componente para manejar el calor
                    varillasColocadas++;
                }
            }
        }
    }
    
    void ActualizarTemperatura()
    {
        if (fuelRodManager == null || 
            fuelRodManager.fuelRods == null || 
            fuelRodManager.fuelRods.Count == 0) return;

        float temperaturaCentral = fuelRodManager.fuelRods[fuelRodManager.fuelRods.Count / 2].temperature;

        foreach (Transform varilla in transform)
        {
            VarillaCalor script = varilla.GetComponent<VarillaCalor>();
            if (script != null)
            {
                script.ActualizarColor(temperaturaCentral);
            }
        }
    }
}

public class VarillaCalor : MonoBehaviour
{
    private Renderer renderer;
    
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }
    
    public void ActualizarColor(float temperatura)
    {
        float t = Mathf.InverseLerp(300, 1000, temperatura); // Normalizar temperatura
        Color color = Color.Lerp(Color.blue, Color.red, t); // Interpolar entre azul y rojo
        renderer.material.color = color;
    }
}



/*using UnityEngine;

public class GeneradorVarillas : MonoBehaviour
{
    public GameObject varillaPrefab; // Prefab de la varilla
    public int totalVarillas = 216;
    private float radioNucleo;
    private float alturaNucleo;
    
    void Start()
    {
        // Obtener el tamaño del cilindro automáticamente
        radioNucleo = GetComponent<Collider>().bounds.extents.x - 5; // Radio basado en el tamaño del cilindro
        alturaNucleo = GetComponent<Collider>().bounds.size.y; // Altura total del cilindro
        GenerarVarillas();
    }

    void GenerarVarillas()
    {
        int filas = Mathf.CeilToInt(Mathf.Sqrt(totalVarillas)); // Determinar el número de filas
        int columnas = totalVarillas / filas;
        float espacioX = (radioNucleo * 2) / columnas; // Espaciado en X basado en el radio
        float espacioZ = (radioNucleo * 2) / filas;    // Espaciado en Z basado en el radio
        
        Vector3 escalaPrefab = varillaPrefab.transform.localScale; // Obtener la escala original del prefab
        Vector3 nuevaEscala = new Vector3(0.01f, 0.85f, 0.01f); // Ajuste manual basado en el prefab en Unity
        float desplazamientoY = nuevaEscala.y + 50f; // Bajar las varillas la misma distancia que su altura
        
        int varillasColocadas = 0;
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                if (varillasColocadas >= totalVarillas) return;
                
                float x = -radioNucleo + (j * espacioX) + (espacioX / 2);
                float z = -radioNucleo + (i * espacioZ) + (espacioZ / 2);
                
                // Verificar que la varilla está dentro del círculo
                if (x * x + z * z <= radioNucleo * radioNucleo)
                {
                    float y = transform.position.y - desplazamientoY; // Bajar las varillas
                    Vector3 posicion = new Vector3(x, y, z) + transform.position;
                    
                    GameObject varilla = Instantiate(varillaPrefab, posicion, Quaternion.identity, transform);
                    varilla.transform.localScale = nuevaEscala; // Aplicar la escala corregida
                    varillasColocadas++;
                }
            }
        }
    }
}
*/