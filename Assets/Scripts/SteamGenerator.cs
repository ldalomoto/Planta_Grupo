using UnityEngine;
using UnityEngine.UI;

public class SteamGenerator : MonoBehaviour
{
    // Variables para el funcionamiento del generador de vapor
    public float waterLevel = 50.0f;           // Nivel de agua inicial
    public float waterInputRate = 5.0f;          // Tasa de entrada de agua
    public float heatTransferRate = 10.0f;       // Tasa de transferencia de calor
    public float temperature = 25.0f;            // Temperatura inicial
    public float maxTemperature = 100.0f;        // Temperatura máxima para generar vapor
    public float steamProductionRate = 10.0f;    // Tasa de producción de vapor
    public float maxSteamPressure = 100.0f;      // Presión máxima de vapor
    public float currentSteamPressure = 0.0f;    // Presión actual de vapor

    public bool isActive = false;              // Estado del generador

    // Referencias a objetos del sistema
    public GameObject feedwaterInlet;          // Entrada de agua
    public GameObject heatExchangeTube;        // Tubos de intercambio de calor
    public GameObject steamOutlet;             // Salida de vapor
    public GameObject collector;               // Colector de vapor
    public ParticleSystem steamParticles;      // Sistema de partículas para vapor

    // Sistema de partículas para simular burbujas en el agua
    public ParticleSystem bubbleParticles;
    public float bubbleTemperatureThreshold = 50.0f; // Temperatura a partir de la cual aparecen las burbujas
    public float maxBubbleEmissionRate = 20.0f;        // Tasa máxima de emisión de burbujas

    // UI para mostrar información
    public Text waterLevelText;
    public Text temperatureText;
    public Text steamPressureText;

    // Objeto visual que representa el agua dentro del cilindro
    public GameObject waterVisual;
    private Renderer waterRenderer;

    void Start()
    {
        if (waterVisual != null)
        {
            waterRenderer = waterVisual.GetComponent<Renderer>();
        }
    }

    void Update()
    {
        if (isActive)
        {
            GenerateSteam();
            UpdateUI();
            UpdateWaterVisual();
            UpdateBubbleEffect();
        }
        else
        {
            if (steamParticles != null && steamParticles.isPlaying)
            {
                steamParticles.Stop();
            }
            if (bubbleParticles != null && bubbleParticles.isPlaying)
            {
                bubbleParticles.Stop();
            }
        }
    }

    /// <summary>
    /// Lógica para el calentamiento, generación de vapor y control del agua.
    /// </summary>
    void GenerateSteam()
    {
        // Entrada de agua
        if (feedwaterInlet != null)
        {
            waterLevel += waterInputRate * Time.deltaTime;
        }

        // Calentamiento del agua
        if (heatExchangeTube != null && waterLevel > 0)
        {
            temperature += heatTransferRate * Time.deltaTime;

            // Cuando se alcanza la temperatura máxima, se produce vapor
            if (temperature >= maxTemperature)
            {
                temperature = maxTemperature;
                currentSteamPressure += steamProductionRate * Time.deltaTime;
                waterLevel -= steamProductionRate * Time.deltaTime;
            }
        }

        // Control del sistema de partículas de vapor
        if (steamOutlet != null && currentSteamPressure > 0)
        {
            if (steamParticles != null && !steamParticles.isPlaying)
            {
                steamParticles.Play();
            }
        }
        else
        {
            if (steamParticles != null && steamParticles.isPlaying)
            {
                steamParticles.Stop();
            }
        }

        // Reducción de presión si se excede el límite
        if (collector != null)
        {
            if (currentSteamPressure > maxSteamPressure)
            {
                currentSteamPressure -= steamProductionRate * Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Actualiza la interfaz gráfica (UI) con el nivel de agua, temperatura y presión de vapor.
    /// </summary>
    void UpdateUI()
    {
        if (waterLevelText != null)
            waterLevelText.text = "Nivel de agua: " + waterLevel.ToString("F2");

        if (temperatureText != null)
            temperatureText.text = "Temperatura: " + temperature.ToString("F2") + " °C";

        if (steamPressureText != null)
            steamPressureText.text = "Presión de vapor: " + currentSteamPressure.ToString("F2") + " kPa";
    }

    /// <summary>
    /// Actualiza la visualización del agua dentro del cilindro.
    /// Se ajusta la escala y posición para que la base del agua permanezca fija.
    /// Además, cambia el color del agua según la temperatura (de azul a rojo).
    /// </summary>
    void UpdateWaterVisual()
    {
        if (waterVisual != null)
        {
            // Limitar el nivel de agua para que no exceda la altura interna del cilindro (suponiendo que 100 es el máximo)
            float nivelAguaClampeado = Mathf.Clamp(waterLevel, 0, 100);
            float nuevaAltura = nivelAguaClampeado / 100f;

            // Escalar el objeto del agua
            //waterVisual.transform.localScale = new Vector3(1, nuevaAltura, 1);
            // Ajustar la posición para que la base se mantenga en Y = 0
            //waterVisual.transform.localPosition = new Vector3(0, nuevaAltura / 2f, 0);

            // Cambiar el color según la temperatura (azul a rojo)
            
            if (waterRenderer != null)
            {
                float t = temperature / maxTemperature;
                Color waterColor = Color.Lerp(Color.blue, Color.red, t);
                waterRenderer.material.color = waterColor;
            }
            
        }
    }

    /// <summary>
    /// Actualiza el sistema de partículas de burbujas.
    /// Las burbujas comienzan a aparecer cuando la temperatura supera el umbral definido.
    /// Además, se ajusta la tasa de emisión de burbujas en función de la temperatura.
    /// </summary>
    void UpdateBubbleEffect()
    {
        if (bubbleParticles != null)
        {
            // Si la temperatura es mayor o igual al umbral, se muestran burbujas
            if (temperature >= bubbleTemperatureThreshold)
            {
                // Se calcula la tasa de emisión proporcional al incremento de temperatura
                var emission = bubbleParticles.emission;
                float bubbleRate = Mathf.Lerp(0, maxBubbleEmissionRate, (temperature - bubbleTemperatureThreshold) / (maxTemperature - bubbleTemperatureThreshold));
                emission.rateOverTime = bubbleRate;

                if (!bubbleParticles.isPlaying)
                {
                    bubbleParticles.Play();
                }
            }
            else
            {
                if (bubbleParticles.isPlaying)
                {
                    bubbleParticles.Stop();
                }
            }
        }
    }

    // Métodos para activar o desactivar el generador
    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }



}
