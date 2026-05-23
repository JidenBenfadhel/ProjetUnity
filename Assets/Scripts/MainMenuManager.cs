using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager1 : MonoBehaviour
{
     public void PlaySolo()
    {
        SceneManager.LoadScene("SampleScene");
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
