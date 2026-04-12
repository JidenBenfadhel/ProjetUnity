using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 1;

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }

        Destroy(gameObject);
    }
}
