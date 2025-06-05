using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawn : MonoBehaviour
{
    public Transform[] Spawn;
    private int playerCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.position = Spawn[0].transform.position;
        playerCount++;
    }
}
