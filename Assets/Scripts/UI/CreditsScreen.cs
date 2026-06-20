using UnityEngine;
using UnityEngine.InputSystem;

public class CreditsScreen : MonoBehaviour
{
    private bool canClose = false;

    void OnEnable()
    {
        canClose = false;
        Invoke(nameof(EnableClosing), 0.1f);
    }

    private void EnableClosing()
    {
        canClose = true;
    }

    void Update()
    {
        if (!canClose) return;

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

        // Si on appuie sur une touche (clavier, souris ou manette)
        if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) ||
            gamepadPressed)
        {
            gameObject.SetActive(false);
        }
    }
}