using UnityEngine;
using System.Collections;

public class PlayerPowerUpManager : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerHealth playerHealth;

    [Header("Auras Visuelles (Glisse les objets ici)")]
    public GameObject speedAura;   // Aura Bleue
    public GameObject bulletAura;  // Aura Verte
    public GameObject shieldAura;  // Aura Rouge

    private float originalMoveSpeed;
    private float originalProjectileSpeed;

    private Coroutine speedCoroutine;
    private Coroutine bulletCoroutine;
    private bool hasShield = false;
    private int lastHealthFrame;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();

        // On éteint toutes les auras au démarrage
        if (speedAura != null) speedAura.SetActive(false);
        if (bulletAura != null) bulletAura.SetActive(false);
        if (shieldAura != null) shieldAura.SetActive(false);
    }

    private void Start()
    {
        // On mémorise les stats de base définies dans l'Inspector
        if (playerController != null)
        {
            originalMoveSpeed = playerController.moveSpeed;
            // Note : Assure-toi que projectileSpeed est bien publique ou accessible dans ton PlayerController
            originalProjectileSpeed = playerController.projectileSpeed; 
        }

        if (playerHealth != null)
        {
            lastHealthFrame = playerHealth.health;
        }
    }

    private void Update()
    {
        if (hasShield && playerHealth != null)
        {
            if (playerHealth.health < lastHealthFrame)
            {
                LoseShield();
            }
            lastHealthFrame = playerHealth.health;
        }
    }

    public void ActivateMoveSpeedBoost(float ratio, float duration)
    {
        if (speedCoroutine != null) StopCoroutine(speedCoroutine);
        speedCoroutine = StartCoroutine(MoveSpeedRoutine(ratio, duration));
    }

    private IEnumerator MoveSpeedRoutine(float ratio, float duration)
    {
        if (playerController != null)
        {
            playerController.moveSpeed = originalMoveSpeed * ratio;
            if (speedAura != null) speedAura.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        if (playerController != null) playerController.moveSpeed = originalMoveSpeed;
        if (speedAura != null) speedAura.SetActive(false);
    }

    public void ActivateBulletSpeedBoost(float ratio, float duration)
    {
        if (bulletCoroutine != null) StopCoroutine(bulletCoroutine);
        bulletCoroutine = StartCoroutine(BulletSpeedRoutine(ratio, duration));
    }

    private IEnumerator BulletSpeedRoutine(float ratio, float duration)
    {
        if (playerController != null)
        {
            playerController.projectileSpeed = originalProjectileSpeed * ratio;
            if (bulletAura != null) bulletAura.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        if (playerController != null) playerController.projectileSpeed = originalProjectileSpeed;
        if (bulletAura != null) bulletAura.SetActive(false);
    }

    public void ActivateShieldBoost()
    {
        if (playerHealth != null)
        {
            playerHealth.health += 1; // Donne le point de vie
            lastHealthFrame = playerHealth.health; // Met à jour le tracker
        }
        
        hasShield = true;
        if (shieldAura != null) shieldAura.SetActive(true);
    }

    private void LoseShield()
    {
        hasShield = false;
        if (shieldAura != null) shieldAura.SetActive(false);
    }
}