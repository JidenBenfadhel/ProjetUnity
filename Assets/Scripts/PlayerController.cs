using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Mouvements")]
    public float moveSpeed = 5f;
    
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
    }

    private void FixedUpdate()
    {
        // On transforme le Vector2 (X, Y) en Vector3 (X, 0, Z) pour la 3D
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
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

    private void Shoot()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}