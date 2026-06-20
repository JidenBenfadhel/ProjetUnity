using UnityEngine;

public class DebrisExplosion : MonoBehaviour
{
    public float explosionForce = 5f;
    public float explosionRadius = 3f;
    public float lifetime = 2f;
    
    private void Start()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.AddExplosionForce(
                explosionForce,
                transform.position,
                explosionRadius,
                0.5f,
                ForceMode.Impulse
            );
        }

        Destroy(gameObject, lifetime);
    }
}
