using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Mouvements")]
    public float moveSpeed = 5f;
    public float turnSpeed = 12f;

    [Header("Cadence de Tir (Arcade)")]
    [Tooltip("Nombre maximum de balles d'affilée avant la surchauffe")]
    public int maxShotsInRow = 3;
    [Tooltip("Temps d'attente (cooldown) après avoir vidé le chargeur")]
    public float burstCooldown = 1.2f;
    [Tooltip("Temps d'attente minimum obligatoire entre deux clics")]
    public float delayBetweenShots = 0.15f;
    [Tooltip("Temps sans tirer nécessaire pour récupérer toutes ses balles automatiquement")]
    public float burstResetDelay = 0.8f;

    [Header("Audio")]
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 0.8f;
    private AudioSource audioSource;

    private int currentShotsFired = 0;
    private float cooldownEndTimestamp = 0f;
    private float lastShotTimestamp = 0f;

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
    private bool isGamepadMode = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
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

        if (Time.time < cooldownEndTimestamp) return;

        if (Time.time < lastShotTimestamp + delayBetweenShots) return;

        if (Time.time > lastShotTimestamp + burstResetDelay)
        {
            currentShotsFired = 0;
        }

        Shoot();
        currentShotsFired++;
        lastShotTimestamp = Time.time;

        if (currentShotsFired >= maxShotsInRow)
        {
            cooldownEndTimestamp = Time.time + burstCooldown;
            currentShotsFired = 0; 
        }
    }
    // -------------------------------

    private void Update()
    {
        float turretRotateInput = 0f;
        if (Gamepad.current != null)
        {
            if (Gamepad.current.xButton.isPressed) turretRotateInput -= 1f; // Bouton X : Tourne a gauche
            if (Gamepad.current.aButton.isPressed) turretRotateInput += 1f; // Bouton A : Tourne a droite
            if (Gamepad.current.bButton.isPressed) turretRotateInput += 1f; // Bouton B : Tourne a droite
            if (Gamepad.current.yButton.isPressed) turretRotateInput -= 1f; // Bouton Y : Tourne a gauche
        }

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 1f)
        {
            isGamepadMode = false;
        }
        if (Mathf.Abs(turretRotateInput) > 0.05f)
        {
            isGamepadMode = true;
        }

        if (isGamepadMode)
        {
            if (Mathf.Abs(turretRotateInput) > 0.05f)
            {
                turretTransform.Rotate(Vector3.up, turretRotateInput * 120f * Time.deltaTime);
            }
        }
        else
        {
            AimWithMouse();
        }

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
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
        }

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}