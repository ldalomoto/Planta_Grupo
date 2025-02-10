// CoolantSystem.cs (básico)
using UnityEngine;

public class CoolantSystem : MonoBehaviour
{
    public float pumpSpeed = 1.0f;
    private float currentFlow = 4.0f; // m³/s
    
    public float GetCoolantFlowRate() => currentFlow * pumpSpeed;
    
    public void SetPumpSpeed(float speed) => pumpSpeed = Mathf.Clamp01(speed);
}