using UnityEngine;

public class DestructibleHealth : MonoBehaviour
{
    [Header("Vie")]
    public int health = 1;

    [Header("Explosion")]
    public GameObject debrisPrefab;

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (debrisPrefab != null)
        {
            Instantiate(debrisPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}
