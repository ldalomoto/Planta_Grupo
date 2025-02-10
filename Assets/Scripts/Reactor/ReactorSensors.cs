// ReactorSensors.cs
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ReactorSensors : MonoBehaviour
{
    [Header("Component References")]
    public FuelRodManager fuelCore;
    public control controlSystem;
    public CoolantSystem coolantSystem; // Asume que tienes un sistema de refrigeración

    [Header("UI Displays")]
    public TMP_Text coreTempDisplay;
    public TMP_Text pressureDisplay;
    public TMP_Text neutronfluxDisplay;
    public TMP_Text coolantFlowDisplay;
    public TMP_Text radiationDisplay;
    public TMP_Text vibrationDisplay;

    [Header("Sensor Parameters")]
    public float maxCoreTemp = 623.15f; // 350°C
    public float maxPressure = 15.5f; // MPa
    public float radiationBaseline = 0.1f; // mSv/h

    // Datos de sensores
    private float coreTemperature;
    private float reactorPressure;
    private float neutronFlux;
    private float coolantFlowRate;
    private float radiationLevel;
    private float vibrationLevel;

    void Update()
    {
        UpdateCoreSensors();
        UpdateCoolantSensors();
        UpdateRadiationSensors();
        UpdateVibrationSensors();
        UpdateUI();
        CheckAlarms();
    }

    void UpdateCoreSensors()
    {
        // Temperatura promedio del núcleo
        float totalTemp = 0f;
        foreach(FuelRod rod in fuelCore.fuelRods){
            totalTemp += rod.temperature;
        }
        coreTemperature = totalTemp / fuelCore.fuelRods.Count;

        // Flujo neutrónico promedio
        neutronFlux = 0f;
        foreach(FuelRod rod in fuelCore.fuelRods){
            neutronFlux += rod.currentFlux;
        }
        neutronFlux /= fuelCore.fuelRods.Count;
    }

    void UpdateCoolantSensors()
    {
        // Simular presión basada en temperatura y posición de las barras
        float pressureFactor = Mathf.Clamp01(coreTemperature / maxCoreTemp);
        float controlRodFactor = 1 - (controlSystem.GetInsertedRodCount() / (float)controlSystem.controlRods.Count);
        
        reactorPressure = Mathf.Lerp(7.0f, maxPressure, pressureFactor * controlRodFactor);
        
        // Flujo del refrigerante (asumiendo sistema de bombeo)
        coolantFlowRate = coolantSystem.GetCoolantFlowRate();
    }

    void UpdateRadiationSensors()
    {
        // Radiación basada en flujo neutrónico y contención
        float leakFactor = Mathf.Clamp01((neutronFlux - 1e15f) / 1e16f);
        radiationLevel = radiationBaseline + (leakFactor * 5.0f); // mSv/h
    }

    void UpdateVibrationSensors()
    {
        // Simular vibración de las bombas
        vibrationLevel = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 2.0f; // mm/s
    }

    void CheckAlarms()
    {
        if(coreTemperature > maxCoreTemp * 0.9f){
            Debug.LogWarning("ALTA TEMPERATURA EN EL NÚCLEO!");
        }
        
        if(reactorPressure > maxPressure * 0.95f){
            Debug.LogWarning("PRESIÓN CRÍTICA EN CIRCUITO PRIMARIO!");
        }
    }

    void UpdateUI()
    {
        coreTempDisplay.text = $"Temperatura Núcleo: {coreTemperature - 273.15:F1} °C";
        pressureDisplay.text = $"Presión: {reactorPressure:F1} MPa";
        neutronfluxDisplay.text = $"Flujo Neutrónico: {neutronFlux / 1e12:F2}×10¹² n/cm²s";
        coolantFlowDisplay.text = $"Flujo Refrigerante: {coolantFlowRate:F2} m³/s";
        radiationDisplay.text = $"Radiación: {radiationLevel:F2} mSv/h";
        vibrationDisplay.text = $"Vibración: {vibrationLevel:F2} mm/s";
    }

    // Métodos para otros sistemas
    public float GetCoreTemperature() => coreTemperature;
    public float GetReactorPressure() => reactorPressure;
    public float GetNeutronFlux() => neutronFlux;
}