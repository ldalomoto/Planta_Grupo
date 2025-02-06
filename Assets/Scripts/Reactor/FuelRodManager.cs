using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;

public class FuelRodManager : MonoBehaviour
{
    public FuelRod[,] fuelRods;
    public int gridSize = 10; // Tamaño de la malla 10x10
    public float reactorPower; // Potencia total del reactor en MW
    public float coolantTemperature;
    public Text neutronFluxText;
    public Text temperatureText;
    public GameObject heatmapQuad;
    private Texture2D heatmapTexture;
    private Color[] heatmapColors;

    void Start()
    {
        // Inicializar la matriz de varillas de combustible
        fuelRods = new FuelRod[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                fuelRods[x, y] = new FuelRod(x, y);
            }
        }

        // Configurar el mapa de calor
        heatmapTexture = new Texture2D(gridSize, gridSize);
        heatmapColors = new Color[gridSize * gridSize];
        heatmapQuad.GetComponent<Renderer>().material.mainTexture = heatmapTexture;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime; 
        Parallel.For(0, gridSize, x =>
        {
            for (int y = 0; y < gridSize; y++)
            {
                fuelRods[x, y].UpdateNeutronFlux(fuelRods, gridSize);
                fuelRods[x, y].CalculateTemperature(coolantTemperature, deltaTime);
            }
        });

        UpdateUI();
        UpdateHeatmap();
    }

    void UpdateUI()
    {
        if (fuelRods.Length > 0)
        {
            float flux = fuelRods[0, 0].neutronFlux;
            float temp = fuelRods[0, 0].temperature;

            Debug.Log("Actualizando UI - Flujo: " + flux + ", Temp: " + temp);

            neutronFluxText.text = "Flujo neutrónico: " + flux.ToString("E2");
            temperatureText.text = "Temperatura: " + temp.ToString("F2") + " K";
        }
    }

    void UpdateHeatmap()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float normalizedFlux = Mathf.InverseLerp(1e14f, 1e16f, fuelRods[x, y].neutronFlux);
                heatmapColors[y * gridSize + x] = Color.Lerp(Color.blue, Color.red, normalizedFlux);
            }
        }
        heatmapTexture.SetPixels(heatmapColors);
        heatmapTexture.Apply();
    }
}