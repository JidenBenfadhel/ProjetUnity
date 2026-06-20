using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public MenuSoundManager soundManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (soundManager != null)
        {
            soundManager.PlayHoverSound();
        }
    }
}