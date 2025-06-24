using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRemove : MonoBehaviour
{
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        Destroy(playerInput.gameObject);
    }
}
