using UnityEngine;

public class ElectricityEffect : MonoBehaviour
{
    public Material cableMaterial; // Asigna tu material aquí
    public float scrollSpeed = 2.0f;

    void Update()
    {
        if (cableMaterial != null)
        {
            float offset = Time.time * scrollSpeed;
            cableMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        }
    }
}

