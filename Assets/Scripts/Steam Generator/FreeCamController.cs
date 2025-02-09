using UnityEngine;

public class FreeCamController : MonoBehaviour
{
    public float moveSpeed = 10f;   // Velocidad de movimiento con teclas
    public float scrollSpeed = 50f; // Velocidad de movimiento con la rueda del mouse
    public float lookSpeed = 2f;    // Sensibilidad del mouse

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        // Movimiento en horizontal (izquierda/derecha)
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

        // Movimiento en vertical (subir/bajar)
        float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        // Movimiento adelante/atrás con la ruedita del mouse
        float moveZ = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;

        // Aplicar movimiento a la posición
        transform.Translate(moveX, moveY, moveZ);

        // Rotación con el mouse
        if (Input.GetMouseButton(1)) // Mantén presionado el botón derecho del mouse
        {
            rotationX += Input.GetAxis("Mouse X") * lookSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -90, 90);
            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
        }
    }
}
