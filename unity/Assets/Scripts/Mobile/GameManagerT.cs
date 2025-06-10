using UnityEngine;
using UnityEngine.InputSystem;

public class GameManagerT : MonoBehaviour
{
    private PlayerInputManager playerInputManager;

    void Awake()
    {
        playerInputManager = FindAnyObjectByType<PlayerInputManager>();
        

        // Always one result
        if (playerInputManager == null)
        {
            Debug.LogError("No PlayerInputManager found in the current scene.");
            return;
        }
    }

    // void OnEnable()
    // {
    //     if (PlayerInputManager.instance != null)
    //     {
    //         PlayerInputManager.instance.onPlayerJoined += OnPlayerJoinedCurrentGame;
    //         PlayerInputManager.instance.onPlayerLeft += OnPlayerLeftCurrentGame;
    //     }
    // }

    // void OnDisable()
    // {
    //     if (PlayerInputManager.instance != null)
    //     {
    //         PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoinedCurrentGame;
    //         PlayerInputManager.instance.onPlayerLeft -= OnPlayerLeftCurrentGame;
    //     }
    // }
    
    void OnDestroy()
    {
        // Important: Unsubscribe when the object is destroyed to prevent memory leaks
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined -= OnPlayerJoinedCurrentGame;
            playerInputManager.onPlayerLeft -= OnPlayerLeftCurrentGame;
            Debug.Log("Unsubscribed from PlayerInputManager.onPlayerJoined event.");
        }
    }
    public void OnPlayerJoinedCurrentGame(PlayerInput playerInput)
    {
        Debug.Log("HELLOO");
        GameObject SpawnOBJ = GameObject.Find("Spawn");
        Transform Spawn = SpawnOBJ.GetComponent<Transform>();
        playerInput.transform.position = Spawn.transform.position;

        // Adding a custom face
        // var face = playerInput.transform.Find("Face");
        // var renderer = face.GetComponent<Renderer>();
        // Texture2D texture = Resources.Load<Texture2D>("");
        // renderer.material.mainTexture = texture;
    }
    public void OnPlayerLeftCurrentGame(PlayerInput playerInput)
    {
        Destroy(playerInput.gameObject);
    }
}
