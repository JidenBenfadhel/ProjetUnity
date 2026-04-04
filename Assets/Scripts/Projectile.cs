using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int maxBounces = 1; 
    private int currentBounces = 0;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Propulse le projectile vers l'avant dès sa création
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si on touche un mur, on compte le rebond
        if (collision.gameObject.CompareTag("Wall"))
        {
            currentBounces++;
            if (currentBounces > maxBounces)
            {
                Destroy(gameObject);
            }
        }
        // Si on touche l'ennemi OU le joueur, on détruit le tank et la balle
        else if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        // Si on touche une autre balle, on détruit les deux balles
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}