using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactorSimulator : MonoBehaviour
{
    public int numFuelRods = 10;  // Número de varillas de combustible
    public float neutronGenerationRate = 2.5f;  // Neutrones generados por fisión (promedio por varilla)
    public Material waterMaterial;  // Material del agua
    public float neutronDecayRate = 0.01f;  // Pérdida de neutrones con el tiempo
    private List<float> neutronCountPerRod;  // Neutrones en cada varilla
    private float totalNeutronsGenerated = 0f;  // Neutrones generados totales
    private float waterTemperature = 300f;  // Temperatura inicial del agua

    void Start()
    {
        neutronCountPerRod = new List<float>(new float[numFuelRods]);
        StartCoroutine(SimulateNeutronGeneration());
        StartCoroutine(LogDebugInfo());
    }

    IEnumerator SimulateNeutronGeneration()
    {
        while (true)
        {
            // Generación de neutrones por cada varilla
            for (int i = 0; i < numFuelRods; i++)
            {
                float generatedNeutrons = neutronGenerationRate * Random.Range(0.8f, 1.2f);
                neutronCountPerRod[i] += generatedNeutrons;
                totalNeutronsGenerated += generatedNeutrons;

                // Simular la propagación de neutrones a varillas cercanas
                PropagateNeutrons(i, generatedNeutrons);
            }

            // Decaimiento natural de neutrones
            for (int i = 0; i < numFuelRods; i++)
            {
                neutronCountPerRod[i] *= (1 - neutronDecayRate);
            }

            // Calcular la temperatura del agua en función de los neutrones generados
            UpdateWaterTemperature();

            // Cambiar color del agua basado en la temperatura
            UpdateWaterColor();

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator LogDebugInfo()
    {
        while (true)
        {
            Debug.Log("Neutrones Generados: " + totalNeutronsGenerated);
            Debug.Log("Temperatura del Agua: " + waterTemperature + "°C");
            yield return new WaitForSeconds(5f);
        }
    }

    void PropagateNeutrons(int sourceIndex, float generatedNeutrons)
    {
        if (sourceIndex < 0 || sourceIndex >= numFuelRods) return;

        for (int i = 0; i < numFuelRods; i++)
        {
            if (i != sourceIndex)
            {
                float neutronTransfer = generatedNeutrons / (Mathf.Abs(sourceIndex - i) + 1);
                neutronCountPerRod[i] += neutronTransfer;
            }
        }
    }

    void UpdateWaterTemperature()
    {
        float neutronEffect = totalNeutronsGenerated * 0.0001f;
        waterTemperature = Mathf.Clamp(300f + neutronEffect, 300f, 330f);
    }

    void UpdateWaterColor()
    {
        if (waterMaterial == null)
        {
            Debug.LogWarning("El material del agua no está asignado.");
            return;
        }

        float normalizedTemp = (waterTemperature - 300f) / (330f - 300f); // Escala de 0 a 1
        waterMaterial.color = Color.Lerp(Color.blue, Color.red, normalizedTemp);
    }
}
