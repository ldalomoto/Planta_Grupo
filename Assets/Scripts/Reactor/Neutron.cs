using UnityEngine;

public class Neutron : MonoBehaviour
{
    public float lifetime = 5f; // Tiempo antes de que el neutrón se elimine
    public float absorptionChance = 0.1f; // Probabilidad de que un neutrón sea absorbido por una varilla de combustible

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FuelRod"))
        {
            // Posibilidad de ser absorbido y generar más neutrones
            if (Random.value < absorptionChance)
            {
                // Aquí podrías generar más neutrones o aumentar temperatura
                Destroy(gameObject);
            }
            else
            {
                // Rebote simple
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
            }
        }
        else if (collision.gameObject.CompareTag("ControlRod"))
        {
            // Absorción total del neutrón
            Destroy(gameObject);
        }
    }
}
 