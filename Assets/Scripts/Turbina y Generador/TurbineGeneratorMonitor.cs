using UnityEngine;
using TMPro; // Para mostrar datos en la UI

public class TurbineGeneratorMonitor : MonoBehaviour
{
    [Header("Datos de la Turbina")]
    public float rpm = 3000f; // Revoluciones por minuto
    public float pressureInput = 5000000f; // Presión de entrada en Pascales (5 MPa)
    public float pressureOutput = 1000000f; // Presión de salida en Pascales (1 MPa)
    public float temperature = 500f; // Temperatura en Kelvin
    public float torque = 500f; // Torque en Nm
    public float gasConstant = 287f; // Constante del gas en J/(kg*K)

    [Header("Datos del Generador")]
    public float voltage = 11000f; // Voltaje en V
    public float current = 200f; // Corriente en A
    public float powerFactor = 0.9f; // Factor de potencia
    public int poles = 4; // Número de polos del generador

    [Header("Textos en UI - Turbina")]
    public TextMeshProUGUI rpmText;
    public TextMeshProUGUI pressureText;
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI massFlowText;
    public TextMeshProUGUI mechanicalPowerText;

    [Header("Textos en UI - Generador")]
    public TextMeshProUGUI electricalPowerText;
    public TextMeshProUGUI efficiencyText;
    public TextMeshProUGUI frequencyText;

    private float timeElapsed = 0f;

    void Update()
    {
        timeElapsed += Time.deltaTime;

        // Simulación de variaciones suaves
        rpm += Mathf.Sin(timeElapsed * 0.5f) * 5f;
        pressureInput += Mathf.Sin(timeElapsed * 0.3f) * 3000f;
        pressureOutput += Mathf.Sin(timeElapsed * 0.4f) * 800f;
        temperature += Mathf.Sin(timeElapsed * 0.2f) * 1.5f;
        torque += Mathf.Sin(timeElapsed * 0.6f) * 3f;
        voltage += Mathf.Sin(timeElapsed * 0.3f) * 30f;
        current += Mathf.Sin(timeElapsed * 0.4f) * 4f;

        // Cálculo del flujo de masa del vapor
        float massFlow = (pressureInput - pressureOutput) / (gasConstant * temperature);

        // Cálculo de potencia mecánica de la turbina
        float angularVelocity = 2 * Mathf.PI * rpm / 60f;
        float mechanicalPower = torque * angularVelocity; // En watts

        // Cálculo de potencia eléctrica del generador
        float electricalPower = voltage * current * powerFactor; // En watts

        // Cálculo de eficiencia
        float efficiency = (mechanicalPower > 0) ? (electricalPower / mechanicalPower) * 100f : 0f;

        // Cálculo de la frecuencia del generador
        float frequency = (rpm * poles) / 120f;

        // Mostrar datos en la UI - Turbina
        if (rpmText) rpmText.text = "Turbina - RPM: " + rpm.ToString("F2");
        if (pressureText) pressureText.text = "Turbina - Presión: " + (pressureInput / 1000000).ToString("F2") + " MPa";
        if (temperatureText) temperatureText.text = "Turbina - Temp: " + temperature.ToString("F2") + " K";
        if (massFlowText) massFlowText.text = "Turbina - Flujo de masa: " + massFlow.ToString("F2") + " kg/s";
        if (mechanicalPowerText) mechanicalPowerText.text = "Turbina - Pot Mecánica: " + (mechanicalPower / 1000).ToString("F2") + " kW";

        // Mostrar datos en la UI - Generador
        if (electricalPowerText) electricalPowerText.text = "Generador - Pot Eléctrica: " + (electricalPower / 1000).ToString("F2") + " kW";
        if (efficiencyText) efficiencyText.text = "Generador - Eficiencia: " + efficiency.ToString("F2") + "%";
        if (frequencyText) frequencyText.text = "Generador - Frecuencia: " + frequency.ToString("F2") + " Hz";
    }
}
