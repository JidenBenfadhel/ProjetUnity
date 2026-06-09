using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager1 : MonoBehaviour
{
    [Header("Transition")]
    public float sceneLoadDelay = 0.25f;

    public void PlaySolo()
    {
        StartCoroutine(LoadSoloSceneAfterDelay());
    }

    private IEnumerator LoadSoloSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene("Level01");
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
