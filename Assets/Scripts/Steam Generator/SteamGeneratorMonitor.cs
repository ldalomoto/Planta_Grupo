using UnityEngine;
using TMPro;

public class SteamGeneratorMonitor : MonoBehaviour
{
    [Header("Datos del Generador de Vapor")]
    public float feedwaterTemperature = 220f; // °C
    public float feedwaterPressure = 6000000f; // Pa (6 MPa)
    public float feedwaterFlowRate = 100f; // kg/s
    public float steamPressure = 5000000f; // Pa (5 MPa)
    public float steamTemperature = 280f; // °C
    public float steamQuality = 0.98f; // Proporción de vapor seco (98%)
    public float efficiency = 90f; // %

    [Header("Textos en UI")]
    public TextMeshProUGUI feedwaterTempText;
    public TextMeshProUGUI feedwaterPressureText;
    public TextMeshProUGUI feedwaterFlowText;
    public TextMeshProUGUI steamPressureText;
    public TextMeshProUGUI steamTempText;
    public TextMeshProUGUI steamQualityText;
    public TextMeshProUGUI efficiencyText;

    private float timeElapsed = 0f;

    void Update()
    {
        timeElapsed += Time.deltaTime;

        // Simulación de variaciones
        feedwaterTemperature += Mathf.Sin(timeElapsed * 0.5f) * 2f;
        feedwaterPressure += Mathf.Sin(timeElapsed * 0.3f) * 50000f;
        feedwaterFlowRate += Mathf.Sin(timeElapsed * 0.4f) * 1f;
        steamPressure += Mathf.Sin(timeElapsed * 0.3f) * 40000f;
        steamTemperature += Mathf.Sin(timeElapsed * 0.2f) * 2f;
        steamQuality += Mathf.Sin(timeElapsed * 0.1f) * 0.005f;
        efficiency += Mathf.Sin(timeElapsed * 0.2f) * 0.5f;

        // Mostrar los datos en la UI
        if (feedwaterTempText) feedwaterTempText.text = "Temp Agua: " + feedwaterTemperature.ToString("F2") + "°C";
        if (feedwaterPressureText) feedwaterPressureText.text = "Presión Agua: " + (feedwaterPressure / 1000000).ToString("F2") + " MPa";
        if (feedwaterFlowText) feedwaterFlowText.text = "Flujo Agua: " + feedwaterFlowRate.ToString("F2") + " kg/s";
        if (steamPressureText) steamPressureText.text = "Presión Vapor: " + (steamPressure / 1000000).ToString("F2") + " MPa";
        if (steamTempText) steamTempText.text = "Temp Vapor: " + steamTemperature.ToString("F2") + "°C";
        if (steamQualityText) steamQualityText.text = "Calidad Vapor: " + (steamQuality * 100).ToString("F2") + "%";
        if (efficiencyText) efficiencyText.text = "Eficiencia: " + efficiency.ToString("F2") + "%";
    }
}
