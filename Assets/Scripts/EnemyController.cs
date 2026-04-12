using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Références")]
    public Transform turretTransform;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Déplacement")]
    public float moveSpeed = 5f;
    public float stopDistance = 6f;

    [Header("Strafe")]
    public float strafeSpeed = 3f;
    public float strafeChangeInterval = 1f;
    private float strafeTimer;
    private int strafeDirection = 1;

    [Header("Tirs")]
    public float fireRate = 2f;
    private float fireTimer;

    private Transform player;
    private Rigidbody rb;

    public void Start() 
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        ChangeStrafeDirection();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

        // Strafing gauche/droite pour plus de réalisme
        strafeTimer += Time.deltaTime;
        if (strafeTimer >= strafeChangeInterval)
        {
            ChangeStrafeDirection();
            strafeTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        Vector3 forwardMove = Vector3.zero;

        // Avance/Recule
        if (distance > stopDistance)
        {
            forwardMove = direction.normalized * moveSpeed;
        }
        else if (distance < stopDistance - 1.5f)
        {
            forwardMove = -direction.normalized * moveSpeed;
        }

        // Strafe
        Vector3 strafeMove = Vector3.Cross(Vector3.up, direction.normalized) * strafeDirection * strafeSpeed;

        Vector3 finalMove = (forwardMove + strafeMove) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + finalMove);
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

    private void ChangeStrafeDirection()
    {
        strafeDirection = Random.value < 0.5f ? -1 : 1;
    }
}
