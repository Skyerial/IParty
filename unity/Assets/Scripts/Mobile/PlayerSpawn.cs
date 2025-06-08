using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class PlayerSpawn : MonoBehaviour
{

    public Transform[] playerSlots;
    private int nextSlotIndex = 0;
    public void OnPlayerJoined(PlayerInput playerInput)
    {

        if (nextSlotIndex >= playerSlots.Length)
        {
            Debug.LogWarning("All player slots are full!");
            return;
        }

        Transform slot = playerSlots[nextSlotIndex];
        playerInput.transform.SetParent(slot);
        playerInput.transform.localPosition = Vector3.zero;
        playerInput.transform.localRotation = Quaternion.identity;
         playerInput.transform.localScale = new Vector3(100f, 100f, 1f);

      
        nextSlotIndex++;
        // GameObject SpawnOBJ = GameObject.Find("Spawn");
        // Transform Spawn = SpawnOBJ.GetComponent<Transform>();
        // playerInput.transform.position = Spawn.transform.position;

        // GameObject playerCard = GameObject.Find("Playercard");
        // if (playerCard != null)
        // {
        //     playerInput.transform.SetParent(playerCard.transform);
        // }
        // else
        // {
        //     Debug.LogWarning("Playercard object not found in the scene.");
        // }

        // // Verander de schaal van de speler
        // playerInput.transform.localScale = new Vector3(100f, 100f, 1000f);




        // Adding a custom face
        // var face = playerInput.transform.Find("Face");
        // var renderer = face.GetComponent<Renderer>();
        // Texture2D texture = Resources.Load<Texture2D>("");
        // renderer.material.mainTexture = texture;
    }
}
