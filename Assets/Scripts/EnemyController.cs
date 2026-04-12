using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Références")]
    public Transform turretTransform;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Tirs")]
    public float fireRate = 2f;
    private float fireTimer;

    public void Start() 
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return; // Si joueur détruit

        // On vise le joueur
        AimAtPlayer();

        // On gère le tir
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate) 
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    private void AimAtPlayer()
    {
        Vector3 targetPosition = player.position;
        targetPosition.y = turretTransform.position.y; // On garde la même hauteur pour éviter que le canon ne se penche vers le bas
        turretTransform.LookAt(targetPosition);
    }

    private void Shoot()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}
