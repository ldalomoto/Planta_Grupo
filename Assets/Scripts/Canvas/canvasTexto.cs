using UnityEngine;

public class canvasTexto : MonoBehaviour {
    public Camera cam;

    private void Update() {
        transform.forward = cam.transform.forward;
    }
}
