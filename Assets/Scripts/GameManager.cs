using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Fin de partie")]
    public GameObject endPanel;
    public TextMeshProUGUI endText;

    [Header("Délais")]
    public float endScreenDelay = 2f; // Le temps d'attente en secondes

    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }

    public void PlayerDied()
    {
        if (gameEnded) return;

        gameEnded = true;
        EndGame("DEFAITE", Color.red);
    }

    public void CheckVictory()
    {
        if (gameEnded) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            gameEnded = true;
            EndGame("VICTOIRE", Color.green);
        }
    }

    private void EndGame(string message, Color color)
    {
        StartCoroutine(EndGameSequence(message, color));
    }

    private IEnumerator EndGameSequence(string message, Color color)
    {
        // On fige l'action immédiatement (destruction des obus, immobilisation des tanks)
        DestroyAllProjectiles();
        FreezeAllTanks();

        // On attend le temps défini dans l'Inspector
        yield return new WaitForSeconds(endScreenDelay);

        // Après le délai, on affiche enfin l'écran de fin
        ShowEndScreen(message, color);

        Invoke(nameof(ReturnToMenu), 3f);
    }

    private void DestroyAllProjectiles()
    {
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject projectile in projectiles)
        {
            Destroy(projectile);
        }
    }

    private void FreezeAllTanks()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in enemies)
        {
            enemy.enabled = false;
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }

    private void ShowEndScreen(string message, Color color)
    {
        if (endPanel != null)
            endPanel.SetActive(true);

        if (endText != null)
        {
            endText.text = message;
            endText.color = color;
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}