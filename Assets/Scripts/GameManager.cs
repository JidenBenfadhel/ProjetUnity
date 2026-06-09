using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes de Niveaux")]
    public string[] levelScenes = { "Level01", "Level02" };

    [Header("UI : Ecran de Transition")]
    public GameObject transitionPanel;
    public TextMeshProUGUI transLevelNameText;
    public TextMeshProUGUI transLevelEnemiesText;
    public TextMeshProUGUI transTotalKillsText;
    public TextMeshProUGUI transPromptText; 

    [Header("UI : HUD de Jeu")]
    public GameObject hudPanel;
    public TextMeshProUGUI hudLevelNameText;
    public TextMeshProUGUI hudEnemiesRemainingText;

    private int currentLevelIndex = 0;
    private int totalEnemiesDefeated = 0;
    private int totalEnemiesInCurrentLevel = 0;
    private bool gameEnded = false;

    private bool isWaitingForInput = false;
    private bool isDefeatScreen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetupUIForMenu();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (isWaitingForInput)
        {
            if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                (Pointer.current != null && Pointer.current.press.wasPressedThisFrame))
            {
                isWaitingForInput = false;
                
                if (isDefeatScreen)
                {
                    isDefeatScreen = false;
                    transitionPanel.SetActive(false);
                    SceneManager.LoadScene("MainMenuScene");
                }
                else
                {
                    StartLevel(); 
                }
            }
        }
    }

    // Declenche au chargement d'une map
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenuScene")
        {
            SetupUIForMenu();
            return;
        }

        StartCoroutine(InitLevelRoutine());
    }

    private IEnumerator InitLevelRoutine()
    {
        gameEnded = false;
        isWaitingForInput = false;

        // Compte les ennemis presents
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalEnemiesInCurrentLevel = enemies.Length;

        // On fige tout au chargement
        FreezeAllTanks(true);

        // On affiche les donnees sur le panneau de transition
        transLevelNameText.text = $"NIVEAU 0{currentLevelIndex + 1}";
        transLevelEnemiesText.text = $"Ennemis dans ce niveau : {totalEnemiesInCurrentLevel}";
        transTotalKillsText.text = $"Ennemis battus a present : {totalEnemiesDefeated}";
        
        UpdateHUD();

        // On affiche le prompt immediatement sur l'ecran beige
        transPromptText.text = "APPUYEZ SUR UNE TOUCHE POUR CONTINUER";
        transPromptText.gameObject.SetActive(true);
        
        isWaitingForInput = true;
        yield return null;
    }

    public void StartNewGame()
    {
        currentLevelIndex = 0;
        totalEnemiesDefeated = 0;
        gameEnded = false;
        LoadNextLevelSequence();
    }

    private void LoadNextLevelSequence()
    {
        if (currentLevelIndex < levelScenes.Length)
        {
            StartCoroutine(TransitionAndLoadRoutine(levelScenes[currentLevelIndex], false));
        }
        else
        {
            StartCoroutine(TransitionAndLoadRoutine("MainMenuScene", true));
        }
    }

    private IEnumerator TransitionAndLoadRoutine(string sceneName, bool isGameFinished)
    {
        hudPanel.SetActive(false);
        transitionPanel.SetActive(true);
        transPromptText.gameObject.SetActive(false); 

        if (sceneName == "MainMenuScene" && isGameFinished)
        {
            transLevelNameText.text = "VICTOIRE TOTALE !";
            transLevelEnemiesText.text = "Felicitations !";
            transTotalKillsText.text = $"Total Ennemis Battus : {totalEnemiesDefeated}";
            transPromptText.text = "APPUYEZ POUR REVENIR AU MENU";
            
            yield return new WaitForSeconds(1.0f);
            isDefeatScreen = true; 
            isWaitingForInput = true;
        }
        else
        {
            transLevelNameText.text = $"NIVEAU 0{currentLevelIndex + 1}";
            transLevelEnemiesText.text = "Preparation de l'arene...";
            transTotalKillsText.text = $"Ennemis battus a present : {totalEnemiesDefeated}";
            
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene(sceneName);
        }
    }

    private void StartLevel()
    {
        // On passe par une Co-routine pour pouvoir integrer le delai de 3 secondes
        StartCoroutine(StartLevelWithDelayRoutine());
    }

    private IEnumerator StartLevelWithDelayRoutine()
    {
        transitionPanel.SetActive(false);
        hudPanel.SetActive(true);

        // On maintient le blocage des tanks pendant que le joueur analyse la scene
        FreezeAllTanks(true);

        // PAUSE DE 3 SECONDES AVANT DE SE LANCER
        yield return new WaitForSeconds(3.0f);

        FreezeAllTanks(false);
    }

    public void PlayerDied()
    {
        if (gameEnded) return;
        gameEnded = true;

        StartCoroutine(DefeatSequence());
    }

    private IEnumerator DefeatSequence()
    {
        FreezeAllTanks(true);
        yield return new WaitForSeconds(2.0f); // 2 secondes de freeze sur ta mort avant l'ecran

        hudPanel.SetActive(false);
        transitionPanel.SetActive(true);

        isDefeatScreen = true;
        transLevelNameText.text = "GAME OVER !";
        transLevelNameText.color = Color.red;
        transLevelEnemiesText.text = $"Tu es mort au Niveau {currentLevelIndex + 1}";
        transTotalKillsText.text = $"Ennemis abattus avant de mourir : {totalEnemiesDefeated}";
        
        transPromptText.text = "APPUYEZ POUR RETOURNER AU MENU";
        transPromptText.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f); 
        isWaitingForInput = true;
    }

    public void CheckVictory()
    {
        if (gameEnded) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int remaining = enemies.Length;

        UpdateHUD();

        if (remaining == 0)
        {
            gameEnded = true;
            totalEnemiesDefeated += totalEnemiesInCurrentLevel;
            currentLevelIndex++;
            StartCoroutine(VictorySequence());
        }
    }

    private IEnumerator VictorySequence()
    {
        FreezeAllTanks(true);
        yield return new WaitForSeconds(2.0f); // Le jeu reste fige sur le terrain pendant 2 secondes de victoire
        
        LoadNextLevelSequence();
    }

    public void UpdateHUD()
    {
        if (hudLevelNameText != null)
            hudLevelNameText.text = $"Niveau 0{currentLevelIndex + 1}";

        if (hudEnemiesRemainingText != null)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            hudEnemiesRemainingText.text = $"Ennemis : {enemies.Length}";
        }
    }

    private void SetupUIForMenu()
    {
        isWaitingForInput = false;
        isDefeatScreen = false;
        transLevelNameText.color = Color.white;
        if (transitionPanel != null) transitionPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    private void FreezeAllTanks(bool freeze)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = !freeze;
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }

        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in enemies)
        {
            enemy.enabled = !freeze;
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
            
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = freeze;
            }
        }
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }
}