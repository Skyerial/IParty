using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class PlayerSpawn : MonoBehaviour
{
   public void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject playersParent = GameObject.Find("Players");
        // if not in lobby. 
        if (playersParent == null)
        {
            GameObject SpawnOBJ = GameObject.Find("Spawn");
            Transform Spawn = SpawnOBJ.GetComponent<Transform>();
            playerInput.transform.position = Spawn.transform.position;
        }
        else
        {
            Transform spawnObj = GameObject.Find("Spawn").transform;

            Transform emptySlot = null;
            for (int i = 0; i < playersParent.transform.childCount; i++)
            {
                Transform playerCard = playersParent.transform.GetChild(i);

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

            playerInput.transform.SetParent(emptySlot, false);
            float offsetY = -emptySlot.GetComponent<RectTransform>().rect.height * 0.30f;
            playerInput.transform.localPosition = new Vector3(0f, offsetY, 0f);
            playerInput.transform.localRotation = Quaternion.identity;
            playerInput.transform.localScale = Vector3.one * 150f;

            spawnObj.SetParent(emptySlot, false);
            spawnObj.localPosition = Vector3.zero;
        }
    }
}