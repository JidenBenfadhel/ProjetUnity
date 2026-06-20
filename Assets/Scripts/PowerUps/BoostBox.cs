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
        GameObject selectedBoost = null;
        if (boosts != null && boosts.Length > 0)
        {
            int randomIndex = Random.Range(0, boosts.Length);
            selectedBoost = boosts[randomIndex];
        }
        if (selectedBoost != null)
        {
            if (discoverSFX != null)
            {
                AudioSource.PlayClipAtPoint(discoverSFX, transform.position, 1.0f);
            }
            // On fait apparaître le bonus proprement
            Instantiate(
                selectedBoost,
                transform.position + Vector3.up * 0.5f,
                Quaternion.identity
            );
        }
        Destroy(gameObject);
    }
}