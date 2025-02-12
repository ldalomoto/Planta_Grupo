using UnityEngine;

public class CamaraController : MonoBehaviour
{
    public float rotationSpeed = 5f; // Velocidad de rotación
    public float dragSpeed = 2f;     // Velocidad de arrastre
    public float zoomSpeed = 10f;    // Velocidad de zoom
    public Transform focusPoint;     // Punto de interés para rotar alrededor (puedes asignar un objeto en la escena)

    private float distanceFromFocus; // Distancia desde el punto de enfoque
    private Vector3 lastMousePosition;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        distanceFromFocus = Vector3.Distance(transform.position, focusPoint.position);
    }

    void Update()
    {
        if (Input.GetMouseButton(1)) // Click derecho: arrastrar cámara
        {
            DragCamera();
        }
        if (Input.GetMouseButton(0)) // Click izquierdo: rotar cámara
        {
            RotateCamera();
        }
        ZoomCamera(); // Rueda del mouse: zoom
    }

    // Mueve la cámara arrastrando con el clic derecho
    void DragCamera()
    {
        Vector3 mouseDelta = (Input.mousePosition - lastMousePosition);
        Vector3 move = new Vector3(-mouseDelta.x * dragSpeed * Time.deltaTime, -mouseDelta.y * dragSpeed * Time.deltaTime, 0);
        transform.Translate(move, Space.World);
        lastMousePosition = Input.mousePosition;
    }

    // Rota la cámara alrededor del punto de interés con el clic izquierdo
    void RotateCamera()
    {
        float h = Input.GetAxis("Mouse X") * rotationSpeed;
        float v = Input.GetAxis("Mouse Y") * rotationSpeed;

        transform.RotateAround(focusPoint.position, Vector3.up, h); // Rota horizontalmente
        transform.RotateAround(focusPoint.position, transform.right, -v); // Rota verticalmente
    }

    // Hace zoom hacia adelante o atrás con la rueda del mouse
    void ZoomCamera()
    {
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        if (zoomInput != 0f)
        {
            distanceFromFocus -= zoomInput * zoomSpeed;
            distanceFromFocus = Mathf.Clamp(distanceFromFocus, 5f, 100f); // Limita el zoom (ajusta los valores según sea necesario)
            Vector3 direction = (transform.position - focusPoint.position).normalized;
            transform.position = focusPoint.position + direction * distanceFromFocus;
        }
    }
}
