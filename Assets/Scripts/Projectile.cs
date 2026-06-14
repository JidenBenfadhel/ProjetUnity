using UnityEngine;

public enum ProjectileOwner
{
    Player,
    Enemy
}

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int maxBounces = 1; 
    private int currentBounces = 0;
    private Rigidbody rb;

    [Header("Owner")]
    public ProjectileOwner owner;

    [Header("Audio")]
    public AudioClip bounceSound;
    [Range(0f, 1f)] public float bounceVolume = 1f;
    public AudioClip explosionSound;
    [Range(0f, 1f)] public float explosionVolume = 0.7f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Propulse le projectile vers l'avant dès sa création
        rb.linearVelocity = transform.forward * speed;
    }

    [Header("Effets Visuels")]
    public GameObject explosionPrefab;

    private void TriggerExplosion()
    {
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si on touche un mur, on compte le rebond
        if (collision.gameObject.CompareTag("Wall"))
        {
            DestructibleHealth destructible = collision.gameObject.GetComponent<DestructibleHealth>();

            if (destructible != null && owner == ProjectileOwner.Player)
            {
                destructible.TakeDamage(1);
    
                TriggerExplosion(); // On fait apparaître l'explosion   
                Destroy(gameObject); // Si ça touche un destructible alors le projecticle est détruit
                return;
            }

            if (bounceSound != null)
            {
                AudioSource.PlayClipAtPoint(bounceSound, transform.position, bounceVolume);
            }

            currentBounces++;
            if (currentBounces > maxBounces)
            {
                Destroy(gameObject);
            }
        }
        
        // Si on touche le player, on réduit la vie du tank player et on détruit la balle
        else if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
            TriggerExplosion();
            Destroy(gameObject);
        }

        // Si on touche l'ennemi, on réduit la vie du tank ennemi et on détruit la balle
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1);
            }

            TriggerExplosion();
            Destroy(gameObject);
        }

        // Si on touche une autre balle, on détruit les deux balles
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody otherRb = other.GetComponentInParent<Rigidbody>();
        
        if (otherRb != null && otherRb.CompareTag("Projectile") && otherRb.gameObject != gameObject)
        {
            // On déclenche l'explosion sur l'autre balle et sur nous-même
            TriggerExplosion(); 
            
            Destroy(otherRb.gameObject);
            Destroy(gameObject);
        }
    }
}