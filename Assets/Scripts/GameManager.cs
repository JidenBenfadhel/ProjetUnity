using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Fin de partie")]
    public GameObject endPanel;
    public TextMeshProUGUI endText;

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

    // On y ajoutera la logique de victoire/défaite plus tard
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
        Debug.Log("Ennemis restants : " + enemies.Length);

        if (enemies.Length == 0)
        {
            gameEnded = true;
            EndGame("VICTOIRE", Color.green);
        }
    }

    private void EndGame(string message, Color color)
    {
        DestroyAllProjectiles();
        FreezeAllTanks();
        ShowEndScreen(message, color);
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
}