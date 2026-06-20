using UnityEngine;

public class BonusLoot : MonoBehaviour
{
    [SerializeField] private Boost boost;

    [SerializeField] private float rotationSpeed = 60f;

    private Vector3 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            boost.Apply(entity);
            Destroy(gameObject);
        }
        
    }
}