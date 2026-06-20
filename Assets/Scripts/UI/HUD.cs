using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI healthText;

    public void UpdateHealth(int health)
    {
        healthText.text = "Vie : " + health;
    }
}