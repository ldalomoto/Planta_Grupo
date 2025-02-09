using UnityEngine;

public class VarillaTemperature : MonoBehaviour
{
    public float baseTemp = 300f;
    public float coolantTemp = 300f;
    public Color hotColor = Color.red;
    public Color coldColor = new Color(0, 0.3f, 1f);

    private Renderer rend;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        UpdateMaterialProperties();
    }

    public void UpdateMaterialProperties()
    {
        rend.GetPropertyBlock(propBlock);
        propBlock.SetFloat("_BaseTemp", baseTemp);
        propBlock.SetFloat("_CoolantTemp", coolantTemp);
        propBlock.SetColor("_HotColor", hotColor);
        propBlock.SetColor("_ColdColor", coldColor);
        rend.SetPropertyBlock(propBlock);
    }
}