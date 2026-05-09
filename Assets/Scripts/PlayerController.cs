using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Mouvements")]
    public float moveSpeed = 5f;
    public float turnSpeed = 12f;

    [Header("Animation & Game Feel")]
    public Transform bodyMesh;
    public float wobbleSpeed = 20f;
    public float wobbleAmount = 1.5f;

    [Header("Chenilles")]
    public Renderer[] trackRenderers; 
    public float trackScrollSpeed = 0.5f;
    public bool scrollXAxis = false;
    
    [Header("Visée et Tir")]
    public Transform turretTransform;
    public LayerMask groundMask;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Rigidbody rb;
    private Vector2 moveInput; // On stocke l'input 2D (ZQSD ou Joystick gauche)
    private Camera mainCam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
    }

    // Cette fonction est détectée automatiquement quand tu utilises les touches de déplacement
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Cette fonction est détectée automatiquement quand tu cliques ou appuies sur la gâchette
    public void OnFire()
    {
        Shoot();
    }
    // -------------------------------

    private void Update()
    {
        AimWithMouse();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            // On calcule l'angle vers lequel le tank DOIT regarder
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            // On fait pivoter le châssis de façon fluide vers ce nouvel angle
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
            // Le tank avance UNIQUEMENT tout droit (dans le sens de ses chenilles)
            // L'utilisation de inputDirection.magnitude permet de garder la sensibilité du joystick
            Vector3 movement = transform.forward * inputDirection.magnitude * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }

    private void AimWithMouse()
    {
        // Sécurité si aucune souris n'est branchée
        if (Mouse.current == null) return; 

        // On récupère la position de la souris avec la nouvelle syntaxe
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);
        
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, groundMask))
        {
            Vector3 targetPosition = hitInfo.point;
            targetPosition.y = turretTransform.position.y;
            turretTransform.LookAt(targetPosition);
        }
    }

    private void HandleAnimation()
    {
        // On vérifie si le joueur touche le joystick / clavier
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Le tremblement Arcade (Wobble)
            if (bodyMesh != null)
            {
                float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
                bodyMesh.localRotation = Quaternion.Euler(0f, 0f, wobble);
            }

            // Le défilement des chenilles (Tapis roulant)
            if (trackRenderers != null && trackRenderers.Length > 0)
            {
                float offset = Time.time * trackScrollSpeed;
                foreach (Renderer rend in trackRenderers)
                {
                    if (rend != null && rend.material != null)
                    {
                        // On fait glisser la texture pour donner l'illusion du mouvement
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
            // Si le tank s'arrête, on remet le châssis droit en douceur
            if (bodyMesh != null)
            {
                bodyMesh.localRotation = Quaternion.Lerp(bodyMesh.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            }
        }
    }

    private void Shoot()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}