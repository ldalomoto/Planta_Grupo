using UnityEngine;

public class TuboGenerador : MonoBehaviour
{
    public float RotationSpeed = 50f; // Velocidad de rotación del tubo

    void Update()
    {
        transform.Rotate(Vector3.forward * RotationSpeed * Time.deltaTime); // Prueba con Vector3.forward si no es el eje correcto
    }
}
