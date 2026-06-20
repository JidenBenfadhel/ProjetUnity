using UnityEngine;

public class ColorSelectionMenu : MonoBehaviour
{
    public GameObject colorSelectionPanel; // Glisse ton panneau UI ici

    // Cette fonction sera appelée par tes boutons de couleur
    public void SelectColorAndStart(string hexColor)
    {
        if (GameManager.Instance != null)
        {
            if (ColorUtility.TryParseHtmlString(hexColor, out Color newColor))
            {
                GameManager.Instance.playerSelectedColor = newColor;
            }

            colorSelectionPanel.SetActive(false);

            GameManager.Instance.StartNewGame();
        }
    }
}