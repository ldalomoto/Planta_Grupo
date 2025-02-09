using UnityEngine;

[RequireComponent(typeof(FuelRod), typeof(Renderer))]
public class FuelRodColorController : MonoBehaviour
{
    [Header("Color Configuration")]
    public Gradient temperatureGradient;
    public float maxReactorRadius = 5f;
    [Range(0f, 1f)]
    public float positionColorIntensity = 0.4f;

    private FuelRod fuelRod;
    private Renderer rodRenderer;
    private Vector3 reactorCenter;

    void Start()
    {
        fuelRod = GetComponent<FuelRod>();
        rodRenderer = GetComponent<Renderer>();
        reactorCenter = transform.parent.position; // Asume que el FuelRodManager es el padre
    }

    void LateUpdate()
    {
        if (fuelRod == null || rodRenderer == null) return;

        // Calcular distancia al centro en el plano XZ
        Vector3 position = transform.position;
        float distance = Vector3.Distance(
            new Vector3(position.x, 0f, position.z),
            new Vector3(reactorCenter.x, 0f, reactorCenter.z)
        );
        
        // Factores de influencia
        float distanceFactor = Mathf.Clamp01(distance / maxReactorRadius);
        float tempNormalized = Mathf.InverseLerp(300f, 1500f, fuelRod.temperature);
        
        // Combinar temperatura real con efecto de posición
        float colorValue = tempNormalized * (1f - distanceFactor * positionColorIntensity);
        colorValue = Mathf.Clamp01(colorValue);
        
        // Aplicar gradiente y variación de saturación
        Color baseColor = temperatureGradient.Evaluate(colorValue);
        Color finalColor = ApplyPositionEffect(baseColor, distanceFactor);
        
        rodRenderer.material.color = finalColor;
    }

    private Color ApplyPositionEffect(Color baseColor, float distanceFactor)
    {
        // Convertir a HSV para ajustar propiedades
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        
        // Aumentar saturación en el centro
        s = Mathf.Clamp01(s * (1.2f - distanceFactor));
        
        // Variar ligeramente el tono
        h = Mathf.Repeat(h + (distanceFactor * 0.1f), 1f);
        
        return Color.HSVToRGB(h, s, v);
    }
}