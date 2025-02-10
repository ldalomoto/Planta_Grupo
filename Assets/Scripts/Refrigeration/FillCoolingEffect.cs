using UnityEngine;

public class FillCoolingEffect : MonoBehaviour
{
    void OnParticleTrigger()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int numParticles = ps.GetParticles(particles);

        for (int i = 0; i < numParticles; i++)
        {
            // Cambiar propiedades cuando entran en el fill
            particles[i].startColor = new Color(0.5f, 0.5f, 1f, 0.8f); // Azul más frío
            particles[i].startSize *= 0.7f; // Hacerlas más pequeñas
            particles[i].velocity *= 0.5f;  // Reducir velocidad para simular enfriamiento
        }

        ps.SetParticles(particles, numParticles);
    }
}
