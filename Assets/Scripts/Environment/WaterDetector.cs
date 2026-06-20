using UnityEngine;

public class WaterDetector : MonoBehaviour
{
    [Header("Effets Spéciaux")]
    [Tooltip("Glisse ici ton préfabriqué de débris ou d'explosion (ex: DebrisExplosion)")]
    public GameObject explosionPrefab; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            TriggerWaterDeath();
        }
    }

    private void TriggerWaterDeath()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, transform.rotation);
        }

        if (CompareTag("Player"))
        {
            gameObject.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied();
            }
        }
        else if (CompareTag("Enemy"))
        {
            gameObject.tag = "Untagged";

            Destroy(gameObject);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckVictory();
            }
        }
    }
}