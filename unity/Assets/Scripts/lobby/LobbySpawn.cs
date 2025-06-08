using UnityEngine;
using UnityEngine.InputSystem;

public class LobbySpawn : MonoBehaviour
{
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject playersParent = GameObject.Find("Players");
        Transform spawnObj = GameObject.Find("Spawn").transform;

        // Find the first empty PlayerCard slot
        Transform emptySlot = null;
        for (int i = 0; i < playersParent.transform.childCount; i++)
        {
            Transform playerCard = playersParent.transform.GetChild(i);
            
            // Consider the slot empty if it has no PlayerInput component among children
            bool hasPlayer = false;
            foreach (Transform child in playerCard)
            {
                if (child.GetComponent<PlayerInput>() != null)
                {
                    hasPlayer = true;
                    break;
                }
            }

            if (!hasPlayer)
            {
                emptySlot = playerCard;
                break;
            }
        }

        if (emptySlot == null)
        {
            Debug.LogWarning("No empty player slot found!");
            return;
        }

        // Attach player to empty slot
        playerInput.transform.SetParent(emptySlot, false);
        float offsetY = -emptySlot.GetComponent<RectTransform>().rect.height * 0.30f;
        playerInput.transform.localPosition = new Vector3(0f, offsetY, 0f);
        playerInput.transform.localRotation = Quaternion.identity;
        playerInput.transform.localScale = Vector3.one * 150f;

        // Move the Spawn object to the next available slot
        spawnObj.SetParent(emptySlot, false);
        spawnObj.localPosition = Vector3.zero;
    }
}
