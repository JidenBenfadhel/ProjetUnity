using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems;

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

    [Header("UI : Écrans Spéciaux")]
    public GameObject howToPlayPanel; 
    public GameObject tipsPanel;

    [Header("UI : Système de Pause")]
    public GameObject pausePanel;         
    public GameObject firstPauseButton;   

    [Header("Configuration Joueur")]
    public Color playerSelectedColor = Color.gray;

    [Header("Audio : Musiques")]
    public AudioClip startGameSFX;   
    public AudioClip bgMusic;        
    [Range(0f, 1f)] public float musicVolume = 0.5f; 
    private AudioSource audioSource;

    [Header("Audio : Événements de Fin")]
    public AudioClip levelVictorySFX;   
    public AudioClip gameVictorySFX;    
    public AudioClip defeatSFX;         
    [Range(0f, 1f)] public float eventSFXVolume = 0.8f;

    // --- VARIABLES DU CHRONOMÈTRE ET RUN ---
    private int currentLevelIndex = 0;
    private int totalEnemiesDefeated = 0;
    private int totalEnemiesInCurrentLevel = 0;
    private bool gameEnded = false;

    private float sessionTimer = 0f;
    private bool isTimerActive = false;

    private bool isWaitingForInput = false;
    private bool isDefeatScreen = false;
    private bool isWaitingForHowToPlay = false;
    private bool isWaitingForTips = false;
    
    private bool isPaused = false; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
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
        if (isTimerActive && !isPaused && !gameEnded)
        {
            sessionTimer += Time.deltaTime;
        }

        if (SceneManager.GetActiveScene().name != "MainMenuScene" && !gameEnded && !isWaitingForHowToPlay && !isWaitingForTips && !isWaitingForInput)
        {
            bool pauseKeyPressed = (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame);
            bool pauseGamepadPressed = (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

            if (pauseKeyPressed || pauseGamepadPressed)
            {
                if (isPaused) ResumeGame();
                else PauseGame();
                return; 
            }
        }

        if (isPaused) return;

        if (isWaitingForInput)
        {
            bool gamepadPressed = false;
            if (Gamepad.current != null)
            {
                if (Gamepad.current.aButton.wasPressedThisFrame ||
                    Gamepad.current.bButton.wasPressedThisFrame ||
                    Gamepad.current.xButton.wasPressedThisFrame ||
                    Gamepad.current.yButton.wasPressedThisFrame ||
                    Gamepad.current.startButton.wasPressedThisFrame)
                {
                    gamepadPressed = true;
                }
            }

            if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) ||
                gamepadPressed)
            {
                if (isWaitingForHowToPlay)
                {
                    isWaitingForHowToPlay = false;
                    isWaitingForInput = false; 
                }
                else if (isWaitingForTips)
                {
                    isWaitingForTips = false;
                    isWaitingForInput = false;
                }
                else if (isDefeatScreen)
                {
                    isDefeatScreen = false;
                    isWaitingForInput = false;
                    transitionPanel.SetActive(false);
                    SceneManager.LoadScene("MainMenuScene");
                }
                else
                {
                    isWaitingForInput = false;
                    StartLevel(); 
                }
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        if (pausePanel != null) pausePanel.SetActive(true);

        if (firstPauseButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstPauseButton);
        }
        
        if (audioSource != null) audioSource.Pause(); 
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        if (pausePanel != null) pausePanel.SetActive(false);
        
        if (audioSource != null) audioSource.UnPause(); 
    }

    public void QuitToMenu()
    {
        isPaused = false;
        isTimerActive = false;
        Time.timeScale = 1f; 
        if (pausePanel != null) pausePanel.SetActive(false);
        SceneManager.LoadScene("MainMenuScene");
    }

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
        isWaitingForHowToPlay = false;
        isWaitingForTips = false;
        isPaused = false;
        Time.timeScale = 1f;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalEnemiesInCurrentLevel = enemies.Length;

        FreezeAllTanks(true);

        if (currentLevelIndex == 0) 
        {
            transitionPanel.SetActive(false);
            
            howToPlayPanel.SetActive(true);
            isWaitingForHowToPlay = true;
            isWaitingForInput = true;
            while (isWaitingForHowToPlay) { yield return null; }
            howToPlayPanel.SetActive(false);

            if (tipsPanel != null)
            {
                tipsPanel.SetActive(true);
                isWaitingForTips = true;
                yield return null; 
                isWaitingForInput = true;
                while (isWaitingForTips) { yield return null; }
                tipsPanel.SetActive(false);
            }
        }

        transitionPanel.SetActive(true);

        transLevelNameText.text = $"NIVEAU 0{currentLevelIndex + 1}";
        transLevelEnemiesText.text = $"Ennemis dans ce niveau : {totalEnemiesInCurrentLevel}";
        transTotalKillsText.text = $"Ennemis battus a present : {totalEnemiesDefeated}";

        UpdateHUD();

        transPromptText.text = "APPUYEZ SUR UNE TOUCHE POUR CONTINUER";
        transPromptText.gameObject.SetActive(true);
        
        isWaitingForInput = true;
    }

    public void StartNewGame()
    {
        currentLevelIndex = 0;
        totalEnemiesDefeated = 0;
        sessionTimer = 0f;
        isTimerActive = false;
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

        if (audioSource != null) audioSource.Stop();

        // ... (haut de la méthode identique)
        if (sceneName == "MainMenuScene" && isGameFinished)
        {
            isTimerActive = false;
            CheckAndSaveBestScore();

            if (audioSource != null && gameVictorySFX != null)
            {
                audioSource.clip = gameVictorySFX;
                audioSource.loop = false;
                audioSource.volume = eventSFXVolume;
                audioSource.Play();
            }

            int bestKills = PlayerPrefs.GetInt("BestKills", 0);
            float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);

            transLevelNameText.text = "VICTOIRE TOTALE !";
            transLevelNameText.color = Color.green;
            
            transLevelEnemiesText.text = $"<b>VOTRE PERFORMANCE :</b>\n" +
                                        $"• Ennemis abattus : {totalEnemiesDefeated}\n" +
                                        $"• Temps total : {FormatTime(sessionTimer)}\n\n" +
                                        $"<b>MEILLEUR RECORD DU JEU :</b>\n" +
                                        $"• Record : {bestKills} Ennemis\n" +
                                        $"• Record de temps : {FormatTime(bestTime)}";

            transTotalKillsText.text = "";

            transPromptText.text = "APPUYEZ POUR REVENIR AU MENU";
            
            yield return new WaitForSeconds(1.0f);
            isDefeatScreen = true; 
            isWaitingForInput = true;
        }
        else
        {
            if (currentLevelIndex > 0 && audioSource != null && levelVictorySFX != null)
            {
                audioSource.clip = levelVictorySFX;
                audioSource.loop = false;
                audioSource.volume = eventSFXVolume;
                audioSource.Play();
            }

            transLevelNameText.text = $"NIVEAU 0{currentLevelIndex + 1}";
            transLevelEnemiesText.text = "Preparation de l'arene...";
            transTotalKillsText.text = $"Ennemis battus a present : {totalEnemiesDefeated}";
            
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene(sceneName);
        }
    }

    private void StartLevel()
    {
        StartCoroutine(StartLevelWithDelayRoutine());
    }

    private IEnumerator StartLevelWithDelayRoutine()
    {
        transitionPanel.SetActive(false);
        hudPanel.SetActive(true);

        if (audioSource != null && startGameSFX != null)
        {
            audioSource.clip = startGameSFX;
            audioSource.loop = false;
            audioSource.volume = musicVolume;
            audioSource.Play();
        }

        FreezeAllTanks(true);
        yield return new WaitForSeconds(3.0f);
        FreezeAllTanks(false);

        isTimerActive = true; 

        if (audioSource != null && bgMusic != null)
        {
            audioSource.clip = bgMusic;
            audioSource.loop = true;
            audioSource.volume = musicVolume;
            audioSource.Play();
        }
    }

    public void PlayerDied()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerActive = false; // Le chrono s'arrête net à la mort
        StartCoroutine(DefeatSequence());
    }

    private IEnumerator DefeatSequence()
    {
        if (audioSource != null) audioSource.Stop();
        FreezeAllTanks(true);
        yield return new WaitForSeconds(2.0f); 

        CheckAndSaveBestScore();

        hudPanel.SetActive(false);
        transitionPanel.SetActive(true);

        if (audioSource != null && defeatSFX != null)
        {
            audioSource.clip = defeatSFX;
            audioSource.loop = false;
            audioSource.volume = eventSFXVolume;
            audioSource.Play();
        }

        int bestKills = PlayerPrefs.GetInt("BestKills", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);

        isDefeatScreen = true;
        transLevelNameText.text = "GAME OVER !";
        transLevelNameText.color = Color.red;
        
        transLevelEnemiesText.text = $"<b>VOTRE PERFORMANCE :</b>\n" +
                                    $"• Mort au Niveau : {currentLevelIndex + 1}\n" +
                                    $"• Ennemis abattus : {totalEnemiesDefeated}\n" +
                                    $"• Temps de survie : {FormatTime(sessionTimer)}\n\n" +
                                    $"<b>MEILLEUR RECORD DU JEU :</b>\n" +
                                    $"• Record : {bestKills} Ennemis\n" +
                                    $"• Record de temps : {FormatTime(bestTime)}";

        transTotalKillsText.text = "";
        
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
            isTimerActive = false;
            totalEnemiesDefeated += totalEnemiesInCurrentLevel;
            currentLevelIndex++;
            StartCoroutine(VictorySequence());
        }
    }

    private IEnumerator VictorySequence()
    {
        if (audioSource != null) audioSource.Stop();
        FreezeAllTanks(true);
        yield return new WaitForSeconds(2.0f);
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
        if (audioSource != null) audioSource.Stop();
        isWaitingForInput = false;
        isDefeatScreen = false;
        isWaitingForTips = false;
        isPaused = false;
        isTimerActive = false;
        Time.timeScale = 1f; 
        transLevelNameText.color = Color.white;
        if (transitionPanel != null) transitionPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (tipsPanel != null) tipsPanel.SetActive(false);
    }

    private void CheckAndSaveBestScore()
    {
        int currentKills = totalEnemiesDefeated;
        float currentTime = sessionTimer;

        // Sécurité : pas de record si on n'a éliminé personne
        if (currentKills <= 0) return;

        int savedBestKills = PlayerPrefs.GetInt("BestKills", 0);
        float savedBestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);

        bool isNewRecord = false;

        if (currentKills > savedBestKills)
        {
            isNewRecord = true;
        }
        else if (currentKills == savedBestKills && currentTime < savedBestTime)
        {
            isNewRecord = true;
        }

        if (isNewRecord)
        {
            PlayerPrefs.SetInt("BestKills", currentKills);
            PlayerPrefs.SetFloat("BestTime", currentTime);
            PlayerPrefs.Save();
        }
    }

    // Formate les secondes brutes en chaîne propre (ex: 02:45)
    private string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds == float.MaxValue || timeInSeconds <= 0f) return "--:--";
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
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