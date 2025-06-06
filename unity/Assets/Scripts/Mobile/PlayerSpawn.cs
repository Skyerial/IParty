using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class PlayerSpawn : MonoBehaviour
{
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject SpawnOBJ = GameObject.Find("Spawn");
        Transform Spawn = SpawnOBJ.GetComponent<Transform>();
        playerInput.transform.position = Spawn.transform.position;

        

        // Adding a custom face
        // var face = playerInput.transform.Find("Face");
        // var renderer = face.GetComponent<Renderer>();
        // Texture2D texture = Resources.Load<Texture2D>("");
        // renderer.material.mainTexture = texture;
    }
}
