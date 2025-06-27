using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRemove : MonoBehaviour
/**
 * @brief Handles disconnection in the mobile testing scene.
 */
{
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        Destroy(playerInput.gameObject);
    }
}
