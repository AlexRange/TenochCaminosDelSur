using UnityEngine;
using UnityEngine.UI;

public class FixedJoystickController : MonoBehaviour
{
    public FixedJoystick fixedJoystick; // Referencia al FixedJoystick del asset store
    public PlayerMove playerController;
    public float sensitivity = 2.0f;

    void Update()
    {
        if (playerController != null && fixedJoystick != null)
        {
            // El FixedJoystick ya maneja su input, solo lo pasamos al player
            playerController.SetHorizontalInput(fixedJoystick.Horizontal * sensitivity);
        }
    }
}