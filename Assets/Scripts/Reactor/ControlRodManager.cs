using UnityEngine;
using System.Collections.Generic;

public class ControlRodManager : MonoBehaviour
{
    public enum MaterialType { BoronCarbide, Cadmium, Hafnium }

    public MaterialType selectedMaterial = MaterialType.BoronCarbide;

    [System.Serializable]
    public class ControlRod
    {
        public Transform rodTransform;
        public bool isInserted = false;
    }

    public List<ControlRod> controlRods;
    private float absorptionEfficiency;

    void Start()
    {
        // Configurar eficiencia de absorción según el material
        switch (selectedMaterial)
        {
            case MaterialType.BoronCarbide:
                absorptionEfficiency = 0.8f;
                break;
            case MaterialType.Cadmium:
                absorptionEfficiency = 0.6f;
                break;
            case MaterialType.Hafnium:
                absorptionEfficiency = 0.7f;
                break;
        }
    }

    public float GetTotalNeutronAbsorption(float totalNeutrons)
    {
        int insertedRods = 0;
        foreach (ControlRod rod in controlRods)
        {
            if (rod.isInserted)
                insertedRods++;
        }

        float absorptionFactor = (float)insertedRods / controlRods.Count;
        float absorbedNeutrons = totalNeutrons * absorptionFactor * absorptionEfficiency;

        return absorbedNeutrons;
    }

    public float GetTotalAbsorption()
    {
        int insertedRods = 0;
        foreach (ControlRod rod in controlRods)
        {
            if (rod.isInserted) insertedRods++;
        }

        // Absorción total basada en el material elegido
        float totalAbsorption = insertedRods * absorptionEfficiency * 1e13f;
        return totalAbsorption;
    }
}
