using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum AIType { Rusher, Sniper, Tactical }
    public enum DifficultyLevel { Level1, Level2, Level3 }

    [Header("Animation & Game Feel")]
    public Transform bodyMesh; 
    public float wobbleSpeed = 20f;
    public float wobbleAmount = 1.5f;

    [Header("Chenilles")]
    public Renderer[] trackRenderers; 
    public float trackScrollSpeed = 0.5f;
    public bool scrollXAxis = false;

    [Header("Configuration IA")]
    public AIType enemyType = AIType.Rusher;
    public DifficultyLevel difficulty = DifficultyLevel.Level1;

    [Header("Références")]
    public Transform turretTransform;
    public Transform firePoint;
    public GameObject standardProjectilePrefab; // Balle normale (0 rebond)
    public GameObject sniperProjectilePrefab;   // Le prefab de projectile spécial pour les snipers, qui peut rebondir une fois.

    [Header("Statistiques de base")]
    public float moveSpeed = 4f;
    public float stopDistance = 2.5f;
    public float fireRate = 2f;
    
    [Header("Paramètres Tactiques")]
    public float searchCoverRadius = 20f; // La distance max à laquelle il cherche un mur
    public float coverOffset = 2.5f;      // La distance derrière le mur où il va se garer
    private float searchTimer = 0f;
    private Vector3 currentCoverPosition;

    private float fireTimer;
    private Transform player;
    private NavMeshAgent agent;

    private bool canShootTarget = false;
    private Vector3 currentAimDirection;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistance;
        agent.updateRotation = false; 
    }

    private void Start() 
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        HandleAiming();
        HandleShooting();
        HandleMovement();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        // On vérifie si l'agent NavMesh est en train de se déplacer
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            // Le tremblement
            if (bodyMesh != null)
            {
                float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
                bodyMesh.localRotation = Quaternion.Euler(0f, 0f, wobble);
            }

            // Le défilement des chenilles
            if (trackRenderers != null && trackRenderers.Length > 0)
            {
                float offset = Time.time * trackScrollSpeed;
                foreach (Renderer rend in trackRenderers)
                {
                    if (rend != null && rend.material != null)
                    {
                        Vector2 currentOffset = rend.material.mainTextureOffset;
                        if (scrollXAxis) 
                            currentOffset.x = offset;
                        else 
                            currentOffset.y = offset;
                            
                        rend.material.mainTextureOffset = currentOffset;
                    }
                }
            }
        }
        else
        {
            if (bodyMesh != null)
            {
                bodyMesh.localRotation = Quaternion.Lerp(bodyMesh.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            }
        }
    }

    private void HandleMovement()
    {
        switch (enemyType)
        {
            case AIType.Rusher:
                MoveTowardsPlayer();
                break;
            case AIType.Sniper:
                MaintainDistance();
                break;
            case AIType.Tactical:
                HideBehindCover();
                break;
        }
    }

    // --- COMPORTEMENT : RUSHER ---
    private void MoveTowardsPlayer()
    {
        agent.SetDestination(player.position);
        OrientChassis();
    }

    // --- COMPORTEMENT : TACTICAL ---
    private void HideBehindCover()
    {
        // On ne recalcule pas la cachette à chaque image (optimisation des performances)
        searchTimer += Time.deltaTime;
        if (searchTimer > 1f) // Calcule une nouvelle cachette toutes les secondes
        {
            FindBestCoverPosition();
            searchTimer = 0f;
        }

        // Si on a trouvé une cachette, on s'y rend
        if (currentCoverPosition != Vector3.zero)
        {
            agent.SetDestination(currentCoverPosition);
            OrientChassis();
        }
    }

    private void FindBestCoverPosition()
    {
        // On crée une sphère imaginaire pour trouver tous les objets autour du tank
        Collider[] colliders = Physics.OverlapSphere(transform.position, searchCoverRadius);
        Transform closestWall = null;
        float minDistance = Mathf.Infinity;

        // On cherche l'objet taggé "Wall" le plus proche
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Wall"))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestWall = col.transform;
                }
            }
        }

        if (closestWall != null)
        {
            // Géométrie : on calcule le vecteur qui va du joueur vers le mur
            Vector3 directionAwayFromPlayer = (closestWall.position - player.position).normalized;
            
            // La cachette idéale est ce mur, repoussée un peu plus loin dans cette même direction
            currentCoverPosition = closestWall.position + directionAwayFromPlayer * coverOffset;
        }
        else
        {
            // S'il n'y a pas de mur, il reste sur place (ou pourrait fuir)
            currentCoverPosition = transform.position;
        }
    }

    // --- COMPORTEMENT : SNIPER ---
    private void MaintainDistance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < stopDistance)
        {
            // On désactive sa distance d'arrêt pour qu'il accepte de rouler vers le point de fuite
            agent.stoppingDistance = 0f;

            Vector3 directionAway = (transform.position - player.position).normalized;
            Vector3 fleePosition = transform.position + directionAway * 6f; 
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleePosition, out hit, 6f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                OrientChassis();
            }
        }
        else if (distanceToPlayer > stopDistance + 4f)
        {
            // On réactive sa distance d'arrêt normale pour qu'il s'arrête de loin
            agent.stoppingDistance = stopDistance;

            agent.SetDestination(player.position);
            OrientChassis();
        }
        else 
        {
            if (agent.hasPath) 
            {
                agent.ResetPath(); 
            }
        }
    }

    // --- MÉTHODES COMMUNES ---
    private void OrientChassis()
    {
        Vector3 direction = agent.velocity;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.1f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }

    private void HandleAiming()
    {
        // On demande au radar de calculer la meilleure direction
        currentAimDirection = FindBestAimDirection(out canShootTarget);

        // On tourne le canon vers cette direction de façon fluide
        if (currentAimDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(currentAimDirection);
            turretTransform.rotation = Quaternion.Slerp(turretTransform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }

    private void HandleShooting()
    {
        fireTimer += Time.deltaTime;
        
        float actualFireRate = fireRate;
        if (difficulty == DifficultyLevel.Level3) actualFireRate = fireRate * 0.5f; 

        if (fireTimer >= actualFireRate)
        {
            if (canShootTarget)
            {
                float angleToTarget = Vector3.Angle(firePoint.forward, currentAimDirection);
                
                if (angleToTarget < 5f)
                {
                    // On choisit la balle en fonction du profil de l'ennemi
                    GameObject prefabToShoot = (enemyType == AIType.Sniper) ? sniperProjectilePrefab : standardProjectilePrefab;
                    
                    // On instancie la munition choisie
                    Instantiate(prefabToShoot, firePoint.position, firePoint.rotation);
                    fireTimer = 0f; 
                }
            }
        }
    }
    private Vector3 FindBestAimDirection(out bool isLocked)
    {
        isLocked = false;
        
        // Direction directe vers le joueur
        Vector3 directAim = (player.position - firePoint.position).normalized;
        directAim.y = 0f;

        Vector3 sphereCastOrigin = firePoint.position + directAim * 0.3f;

        // Le "0.3f" correspond au rayon de la sphère (l'épaisseur estimée de l'obus).
        if (Physics.SphereCast(sphereCastOrigin, 0.3f, directAim, out RaycastHit directHit, 50f))
        {
            if (directHit.collider.CompareTag("Player"))
            {
                isLocked = true;
                return directAim;
            }
        }
        // Test du tir avec rebond (Spécial Sniper)
        if (enemyType == AIType.Sniper)
        {
            // On lance 360 rayons en cercle (un tous les 1 degré)
            for (int i = 0; i < 360; i++)
            {
                float angle = i * 1f;
                Vector3 testDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                if (Physics.Raycast(firePoint.position, testDirection, out RaycastHit hit, 50f))
                {
                    if (hit.collider.CompareTag("Wall"))
                    {
                        Vector3 reflectDir = Vector3.Reflect(testDirection, hit.normal);
                        reflectDir.y = 0f;
                        
                        // Rebond (décalé de 0.5f pour ne pas retoucher le mur)
                        if (Physics.Raycast(hit.point + reflectDir * 0.5f, reflectDir, out RaycastHit bounceHit, 50f))
                        {
                            if (bounceHit.collider.CompareTag("Player"))
                            {
                                isLocked = true;
                                return testDirection; 
                            }
                            // Si le rebond risque de toucher un "Enemy", il l'ignorera aussi
                        }
                    }
                }
            }
        }
        
        // Si aucun tir propre n'est possible, on regarde quand même dans la direction du joueur pour être prêt à tirer dès que l'allié s'écartera
        return directAim;
    }
}