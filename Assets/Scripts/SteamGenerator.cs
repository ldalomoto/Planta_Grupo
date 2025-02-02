using UnityEngine;
using UnityEngine.UI;

public class SteamGenerator : MonoBehaviour
{
    public float waterLevel = 50.0f; // Nivel de agua inicial
    public float waterInputRate = 5.0f; // Tasa de entrada de agua
    public float heatTransferRate = 10.0f; // Tasa de transferencia de calor
    public float temperature = 25.0f; // Temperatura inicial del agua
    public float maxTemperature = 100.0f; // Temperatura máxima para generar vapor
    public float steamProductionRate = 10.0f; // Tasa de producción de vapor
    public float maxSteamPressure = 100.0f; // Presión máxima de vapor
    public float currentSteamPressure = 0.0f; // Presión actual de vapor

    public bool isActive = false; // Estado del generador

    // Referencias a objetos
    public GameObject feedwaterInlet; // Entrada de agua
    public GameObject heatExchangeTube; // Tubos de intercambio de calor
    public GameObject steamOutlet; // Salida de vapor
    public GameObject collector; // Colector de vapor
    public ParticleSystem steamParticles; // Sistema de partículas de vapor

    // UI
    public Text waterLevelText;
    public Text temperatureText;
    public Text steamPressureText;

    // Objeto visual del agua (para cambiar tamaño/color)
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
        }
        else
        {
            if (steamParticles != null && steamParticles.isPlaying)
            {
                steamParticles.Stop();
            }
        }
    }

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
            if (temperature > maxTemperature)
            {
                temperature = maxTemperature;
                // Generación de vapor
                currentSteamPressure += steamProductionRate * Time.deltaTime;
                waterLevel -= steamProductionRate * Time.deltaTime;
            }
        }

        // Control del vapor
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

        // Control de presión
        if (collector != null)
        {
            if (currentSteamPressure > maxSteamPressure)
            {
                currentSteamPressure -= steamProductionRate * Time.deltaTime;
            }
        }
    }

    void UpdateUI()
    {
        if (waterLevelText != null)
            waterLevelText.text = "Nivel de agua: " + waterLevel.ToString("F2");

        if (temperatureText != null)
            temperatureText.text = "Temperatura: " + temperature.ToString("F2") + "°C";

        if (steamPressureText != null)
            steamPressureText.text = "Presión de vapor: " + currentSteamPressure.ToString("F2") + " kPa";
    }

    void UpdateWaterVisual()
    {
        if (waterVisual != null)
        {
            // Ajustar altura del agua
            waterVisual.transform.localScale = new Vector3(1, waterLevel / 100f, 1);

            // Cambio de color según temperatura
            if (waterRenderer != null)
            {
                float t = temperature / maxTemperature;
                Color waterColor = Color.Lerp(Color.blue, Color.red, t);
                waterRenderer.material.color = waterColor;
            }
        }
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
