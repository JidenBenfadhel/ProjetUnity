using UnityEngine;
using UnityEngine.InputSystem;

public class DynamicCamera : MonoBehaviour
{
    [Header("Cible")]
    public Transform player;

    [Header("Mouvement de Suivi")]
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0f, 12f, -12f); 

    [Header("Rotation Orbitale")]
    public float rotationSpeed = 100f;       
    public float rotationSmoothTime = 8f;    
    public float pitchAngle = 45f;           

    [Header("Limites de l'arène (Bords)")]
    public Vector2 minBounds; 
    public Vector2 maxBounds; 

    private Vector3 velocity = Vector3.zero;
    private float currentRotationAngle = 0f;

    private void Start()
    {
        currentRotationAngle = transform.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        HandleRotationInput();

        Quaternion camTurnRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);
        Vector3 rotatedOffset = camTurnRotation * offset;
        Vector3 desiredPosition = player.position + rotatedOffset;
        if (minBounds != Vector2.zero || maxBounds != Vector2.zero)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        Quaternion targetRotation = Quaternion.Euler(pitchAngle, currentRotationAngle, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothTime);
    }

    private void HandleRotationInput()
    {
        float rotInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.isPressed) rotInput -= 1f; // Touche A
            if (Keyboard.current.eKey.isPressed) rotInput += 1f; // Touche E
        }

        // ================= LECTURE DE LA MANETTE =================
        if (Gamepad.current != null)
        {
            // Lecture du Stick Droit (Axe Horizontal)
            float stickX = Gamepad.current.rightStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.1f) // Deadzone pour éviter la dérive du stick
            {
                rotInput += stickX;
            }

            // Rotation avec les Bumpers (LB / RB ou L1 / R1)
            if (Gamepad.current.leftShoulder.isPressed) rotInput -= 1f;
            if (Gamepad.current.rightShoulder.isPressed) rotInput += 1f;
        }

        // Application de la rotation
        currentRotationAngle += rotInput * rotationSpeed * Time.deltaTime;
    }
}