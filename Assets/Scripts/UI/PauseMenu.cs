using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    // Appelé par le bouton REPRENDRE
    public void Btn_Resume()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    // Appelé par le bouton QUITTER
    public void Btn_Quit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitToMenu();
        }
    }
}