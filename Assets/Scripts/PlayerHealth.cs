using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 1;
    
    [Header("Effets Visuels")]
    public GameObject explosionPrefab;

    public void TakeDamage(int damage)
    {  
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded()) return;

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // On fait apparaître l'explosion à la position exacte du tank
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
        Destroy(gameObject);
    }
}