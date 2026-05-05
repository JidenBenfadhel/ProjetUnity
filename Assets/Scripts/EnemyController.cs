using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum AIType { Rusher, Sniper, Tactical }
    public enum DifficultyLevel { Level1, Level2, Level3 }

    [Header("Configuration IA")]
    public AIType enemyType = AIType.Rusher;
    public DifficultyLevel difficulty = DifficultyLevel.Level1;

    [Header("Références")]
    public Transform turretTransform;
    public Transform firePoint;
    public GameObject projectilePrefab;

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
            // On vérifie que le radar a validé une cible
            if (canShootTarget)
            {
                // Vérification cruciale : On s'assure que le canon a eu le temps de tourner 
                // et qu'il est bien aligné avec l'angle calculé avant de tirer (marge d'erreur de 5 degrés)
                float angleToTarget = Vector3.Angle(firePoint.forward, currentAimDirection);
                
                if (angleToTarget < 5f)
                {
                    Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                    fireTimer = 0f; // Le tir est parti, on remet le chrono à zéro
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

        // Test du tir direct (Ligne de vue dégagée)
        if (Physics.Raycast(firePoint.position, directAim, out RaycastHit directHit, 50f))
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
                // On crée un vecteur de direction basé sur l'angle
                Vector3 testDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                // On tire le premier rayon
                if (Physics.Raycast(firePoint.position, testDirection, out RaycastHit hit, 50f))
                {
                    // S'il touche un mur, on calcule le rebond
                    if (hit.collider.CompareTag("Wall"))
                    {
                        Vector3 reflectDir = Vector3.Reflect(testDirection, hit.normal);
                        reflectDir.y = 0f;
                        // On relance un second rayon depuis le mur pour voir où va le rebond
                        // (On décale le point de départ de 0.5f pour ne pas retoucher le mur lui-même)
                        if (Physics.Raycast(hit.point + reflectDir * 0.5f, reflectDir, out RaycastHit bounceHit, 50f))
                        {
                            if (bounceHit.collider.CompareTag("Player"))
                            {
                                isLocked = true;
                                return testDirection; 
                            }
                        }
                    }
                }
            }
        }
        // Si aucun tir n'est possible, on regarde quand même dans la direction du joueur
        return directAim;
    }
}