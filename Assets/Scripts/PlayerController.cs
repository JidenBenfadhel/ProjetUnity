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
        if (!enabled) return;
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
        // On récupère l'orientation actuelle de la caméra dans l'espace mécanique
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        // On aplatit ces vecteurs sur l'axe Y (vertical) 
        // pour éviter que le tank ne veuille s'enfoncer dans le sol en voulant avancer
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // On recalcule la direction de mouvement par rapport à l'axe de la caméra
        // (L'avant de l'écran * ton stick vertical + la droite de l'écran * ton stick horizontal)
        Vector3 inputDirection = (camForward * moveInput.y) + (camRight * moveInput.x);

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

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