using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 1;
    private HUD hud;

    private void Start()
    {
        hud =FindObjectOfType<HUD>();

        if (hud != null)
            hud.UpdateHealth(health);
    }
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
