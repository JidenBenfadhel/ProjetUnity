using UnityEngine;

public class EnemyHealth : MonoBehaviour
{  
    public int health = 3;

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
        gameObject.tag = "Untagged";
        Destroy(gameObject);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckVictory();
        }
    }
}
