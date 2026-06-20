using UnityEngine;

public class BonusLoot : MonoBehaviour
{
    [SerializeField] private Boost boost;
    [SerializeField] private float rotationSpeed = 60f;
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSFX;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider entity)
    {
        if (entity.CompareTag("Player") && boost != null)
        {
            // On joue le son de récupération
            if (pickupSFX != null)
            {
                AudioSource.PlayClipAtPoint(pickupSFX, transform.position, 1.0f);
            }

            boost.Apply(entity);
            Destroy(gameObject);
        }
    }
}