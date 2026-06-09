using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Transition")]
    public float sceneLoadDelay = 0.25f;

    public void PlaySoloGame()
    {
        StartCoroutine(LoadSoloSceneAfterDelay());
    }

    private IEnumerator LoadSoloSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
        else
        {
            Debug.LogError("Le GameManager est introuvable !");
        }
     }  

    public void OpenMultiplayer()
    {
        Debug.Log("Multijoueur pas encore disponible");
    }

    public void OpenCredits()
    {
        Debug.Log("Crédits pas encore disponible");
    }

    public void OpenShop()
    {
        Debug.Log("Boutique pas encore disponible");
    }
}
