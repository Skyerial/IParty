using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.TextCore;
using UnityEngine.UI;
using System;
using TMPro;

public class PlayerSpawn : MonoBehaviour
{

    void Start()
    {
        //Remove players from the lobby scene 
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Lobby") ServerManager.allControllers?.Clear();
    }

    Material findColor(InputDevice device)
    {
        Material mat = Resources.Load<Material>("Materials/Default");
        switch (PlayerManager.playerStats[device].color)
        {
            case "Yellow":
                mat = Resources.Load<Material>("Materials/Global/Yellow");
                break;
            case "Red":
                mat = Resources.Load<Material>("Materials/Global/Red");
                break;
            case "Green":
                mat = Resources.Load<Material>("Materials/Global/Green");
                break;
            case "Blue":
                mat = Resources.Load<Material>("Materials/Global/Blue");
                break;
        }
        return mat;
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

                    // TESTING PLAYER MANAGER
                    TextMeshProUGUI nameField = characterFrame.Find("PlayerName").Find("PlayerName").GetComponent<TextMeshProUGUI>();
                    // TextMeshProUGUI nameField = nameObject.GetComponent<TextMeshProUGUI>();
                    string playerName = PlayerManager.playerStats[playerInput.devices[0]].name;
                    nameField.text = playerName;
                    characterFrame.gameObject.SetActive(true);
                    break;
                }

            }

            if (emptySlot == null)
            {
                Debug.LogWarning("No empty player slot found!");
                return;
            }

            // TESTING PLAYER MANAGER
            Transform body = playerInput.transform.Find("Body");
            SkinnedMeshRenderer renderer = body.GetComponent<SkinnedMeshRenderer>();
            renderer.material = findColor(playerInput.devices[0]);


            playerInput.transform.SetParent(emptySlot, false);
            float offsetY = -emptySlot.GetComponent<RectTransform>().rect.height * 0.15f;
            playerInput.transform.localPosition = new Vector3(0f, offsetY, 0f);
            playerInput.transform.localRotation = Quaternion.identity;
            playerInput.transform.localScale = Vector3.one * 150f;
            playerInput.DeactivateInput();
        }
    }
}