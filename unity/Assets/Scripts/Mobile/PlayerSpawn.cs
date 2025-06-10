using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.TextCore;
using UnityEngine.UI;
using System;

public class PlayerSpawn : MonoBehaviour
{

    void Start()
    {
        //Remove players from the lobby scene 
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Lobby") ServerManager.allControllers?.Clear();
    }
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
            Transform emptySlot = null;
            for (int i = 0; i < playersParent.transform.childCount; i++)
            {
                Transform playerCard = playersParent.transform.GetChild(i);
                Transform playerWaitingFrame = playerCard.Find("waitingFrame");
                Transform characterFrame = playerCard.Find("PlayerCharacterFrame");
                Transform characterShow = characterFrame.Find("characterShow");
                bool hasPlayer = false;
                foreach (Transform child in characterShow)
                {
                    if (child.GetComponent<PlayerInput>() != null)
                    {
                        Debug.Log("Player found in characterShow");
                        hasPlayer = true;
                        break;
                    }
                }

                if (!hasPlayer)
                {
                    emptySlot = characterShow;
                    Debug.Log("Player not found in characterShow, looking for empty slot");
                    playerWaitingFrame.gameObject.SetActive(false);
                    characterFrame.gameObject.SetActive(true);
                    break;
                }

            }

            if (emptySlot == null)
            {
                Debug.LogWarning("No empty player slot found!");
                return;
            }
            playerInput.transform.SetParent(emptySlot, false);
            float offsetY = -emptySlot.GetComponent<RectTransform>().rect.height * 0.15f;
            playerInput.transform.localPosition = new Vector3(0f, offsetY, 0f);
            playerInput.transform.localRotation = Quaternion.identity;
            playerInput.transform.localScale = Vector3.one * 150f;
        }
    }
}