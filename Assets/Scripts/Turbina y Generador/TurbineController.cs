using UnityEngine;

public class TurbineController : MonoBehaviour
{
    public ParticleSystem centralSteam; // Vapor central
    public ParticleSystem[] sideSteams; // Otros tres sistemas de part�culas
    public HelixRotation helix1; // Script de rotaci�n de h�lices turbina 1
    public HelixRotation helix2; // Script de rotaci�n de h�lices turbina 2
    public HeliceGenerador helice; //Script de rotaci�n de h�lices generador el�ctrico
    public TubeRotation tube; // Script de rotaci�n del tubo turbina
    public TuboGenerador tubo; // Script de rotaci�n del tubo generador el�ctrico

    public float steamDelay = 1f; // Retraso antes de que salga el vapor central
    public float rotationDelay = 2f; // Retraso antes de que las h�lices y el tubo empiecen a girar
    public float sideSteamDelay = 4f; // Retraso antes de que salgan los vapores laterales
    public float rotationDelayGen = 2f; // Retraso antes de que el generador gire

    public bool isActivated = false; // Casilla de verificaci�n para activar/desactivar la simulaci�n

    void Update()
    {
        if (isActivated || !isActivated)
        {
            // Iniciar la simulaci�n si est� activada
            if (!centralSteam.isPlaying)
            {
                StartSimulation();
            }
        }
        else
        {
            // Detener la simulaci�n si est� desactivada
            if (centralSteam.isPlaying)
            {
                StopSimulation();
            }
        }
    }

    void StartSimulation()
    {
        // Iniciar la secuencia de simulaci�n
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

        // Detener la rotaci�n de las h�lices y tubo
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
        if (helix1) helix1.enabled = true; // Iniciar rotaci�n de la turbina 1
        if (helix2) helix2.enabled = true; // Iniciar rotaci�n de la turbina 2
        if (tube) tube.enabled = true;     // Iniciar rotaci�n del tubo
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
        if (helice) helice.enabled = true; // Iniciar rotaci�n de la turbina generador electrico
        if (tubo) tubo.enabled = true;     // Iniciar rotaci�n del tubo generador electrico
    }
}
