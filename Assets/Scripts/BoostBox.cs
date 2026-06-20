using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private GameObject[] boosts;
    [Header("Audio")]
    [SerializeField] private AudioClip discoverSFX;

    private void OnCollisionEnter(Collision collision)
    {
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();

        if (projectile != null && projectile.owner == ProjectileOwner.Player)
        {
            DestroyBox();
            Destroy(projectile.gameObject);
        }
    }

    private void DestroyBox()
    {
        // On joue le son pile a l'emplacement de la caisse avant sa destruction
        if (discoverSFX != null)
        {
            AudioSource.PlayClipAtPoint(discoverSFX, transform.position, 1.0f);
        }

        SpawnRandomBoost(); 
        Destroy(gameObject);
    }

    private void SpawnRandomBoost()
    {
        if (boosts.Length != 0)
        {
            int randomIndex = Random.Range(0, boosts.Length);

            Instantiate(
                boosts[randomIndex],
                transform.position + Vector3.up * 0.5f,
                Quaternion.identity
            );
        }
    }
}