using UnityEngine;

public class HeliceGenerador : MonoBehaviour
{
    public float RotationSpeed = 200f; // Velocidad de rotación propia

    void Update()
    {
        transform.Rotate(Vector3.right * RotationSpeed * Time.deltaTime); // Prueba con Vector3.up si no gira bien
    }
}
