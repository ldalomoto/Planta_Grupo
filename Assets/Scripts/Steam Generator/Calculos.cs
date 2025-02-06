using UnityEngine;
using UnityEngine.UI;

public class Calculos : MonoBehaviour
{
    private Text waterLevelText;
    private Text temperatureText;
    private Text steamPressureText;

    void Start()
    {
        // Busca los objetos en la jerarquía
        waterLevelText = GameObject.Find("WaterLevelText")?.GetComponent<Text>();
        temperatureText = GameObject.Find("TemperatureText")?.GetComponent<Text>();
        steamPressureText = GameObject.Find("SteamPressureText")?.GetComponent<Text>();

        // Verifica si encontró los objetos correctamente
        if (waterLevelText == null || temperatureText == null || steamPressureText == null)
        {
            Debug.LogError("Uno o más componentes Text no fueron encontrados. Verifica los nombres en la jerarquía.");
            return;
        }

        // Inicializa los valores si todo está correcto
        UpdateWaterLevel(50.0f);
        UpdateTemperature(300.0f);
        UpdateSteamPressure(15.0f);
    }

    public void UpdateWaterLevel(float level)
    {
        if (waterLevelText != null)
            waterLevelText.text = "Water Level: " + level.ToString("F1") + "%";
    }

    public void UpdateTemperature(float temp)
    {
        if (temperatureText != null)
            temperatureText.text = "Temperature: " + temp.ToString("F1") + "°C";
    }

    public void UpdateSteamPressure(float pressure)
    {
        if (steamPressureText != null)
            steamPressureText.text = "Steam Pressure: " + pressure.ToString("F1") + " bar";
    }
}
