using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawn : MonoBehaviour
{
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject SpawnOBJ = GameObject.Find("Spawn");
        Transform Spawn = SpawnOBJ.GetComponent<Transform>();
        playerInput.transform.position = Spawn.transform.position;

        // Adding a custom color to body
        // var body = playerInput.transform.Find("Body");
        // var render = body.GetComponent<Renderer>();
        // Material mat = Resources.Load<Material>("Materials/Global/Blue");
        // render.material = mat;


        // Adding a custom face
        // var face = playerInput.transform.Find("Face");
        // var renderer = face.GetComponent<Renderer>();
        // Texture2D texture = Resources.Load<Texture2D>("");
        // renderer.material.mainTexture = texture;
    }
}
