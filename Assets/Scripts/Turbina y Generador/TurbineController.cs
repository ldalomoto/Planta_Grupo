using UnityEngine;

public class TurbineController : MonoBehaviour
{
    public ParticleSystem centralSteam; // Vapor central
    public ParticleSystem[] sideSteams; // Otros tres sistemas de partículas
    public HelixRotation helix1; // Script de rotación de hélices turbina 1
    public HelixRotation helix2; // Script de rotación de hélices turbina 2
    public HeliceGenerador helice; //Script de rotación de hélices generador eléctrico
    public TubeRotation tube; // Script de rotación del tubo turbina
    public TuboGenerador tubo; // Script de rotación del tubo generador eléctrico

    public float steamDelay = 1f; // Retraso antes de que salga el vapor central
    public float rotationDelay = 2f; // Retraso antes de que las hélices y el tubo empiecen a girar
    public float sideSteamDelay = 4f; // Retraso antes de que salgan los vapores laterales
    public float rotationDelayGen = 2f; // Retraso antes de que el generador gire

    public bool isActivated = false; // Casilla de verificación para activar/desactivar la simulación

    void Update()
    {
        if (isActivated)
        {
            // Iniciar la simulación si está activada
            if (!centralSteam.isPlaying)
            {
                StartSimulation();
            }
        }
        else
        {
            // Detener la simulación si está desactivada
            if (centralSteam.isPlaying)
            {
                StopSimulation();
            }
        }
    }

    void StartSimulation()
    {
        // Iniciar la secuencia de simulación
        Invoke("ActivateCentralSteam", steamDelay);
        Invoke("StartRotation", rotationDelay);
        Invoke("ActivateSideSteams", sideSteamDelay);
        Invoke("StartRotationGen", rotationDelayGen);
    }

    void StopSimulation()
    {
        // Detener todos los efectos
        if (centralSteam) centralSteam.Stop();
        foreach (var steam in sideSteams)
        {
            if (steam) steam.Stop();
        }

        // Detener la rotación de las hélices y tubo
        if (helix1) helix1.enabled = false;
        if (helix2) helix2.enabled = false;
        if (helice) helice.enabled = false;
        if (tube) tube.enabled = false;
        if (tubo) tubo.enabled = false;
    }

    void ActivateCentralSteam()
    {
        if (centralSteam)
        {
            var main = centralSteam.main;
            main.startSpeed = 20f; // Ajusta la velocidad del vapor central
            centralSteam.Play();
        }
    }

    void StartRotation()
    {
        if (helix1) helix1.enabled = true; // Iniciar rotación de la turbina 1
        if (helix2) helix2.enabled = true; // Iniciar rotación de la turbina 2
        if (tube) tube.enabled = true;     // Iniciar rotación del tubo
    }

    void ActivateSideSteams()
    {
        foreach (var steam in sideSteams)
        {
            if (steam)
            {
                steam.Play();
            }
        }
    }

    void StartRotationGen()
    {
        if (helice) helice.enabled = true; // Iniciar rotación de la turbina generador electrico
        if (tubo) tubo.enabled = true;     // Iniciar rotación del tubo generador electrico
    }
}
