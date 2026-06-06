using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlaySoloGame()
{
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
