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
    int i = 0;
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
            // Only works for standard prefab.
            // Transform body = playerInput.transform.Find("Body");
            // SkinnedMeshRenderer renderer = body.GetComponent<SkinnedMeshRenderer>();
            // renderer.material = PlayerManager.findColor(playerInput.devices[0]);
            GameObject SpawnOBJ = GameObject.Find("Spawn");
            Transform[] Spawn = SpawnOBJ.GetComponentsInChildren<Transform>()
                                        .Where(t => t != SpawnOBJ.transform)
                                        .ToArray();

            int spawnIndex = i % Spawn.Length;
            playerInput.transform.position = Spawn[spawnIndex].position;
            playerInput.transform.rotation = Spawn[spawnIndex].rotation;
            Debug.Log($"welke spawn object {spawnIndex}");
            Debug.Log(Spawn[spawnIndex].name);
            i++;

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
            renderer.material = PlayerManager.findColor(playerInput.devices[0]);

            // TESTING FACES
            Transform face = playerInput.transform.Find("Face");
            SkinnedMeshRenderer renderer_face = face.GetComponent<SkinnedMeshRenderer>();
            Texture2D faceTexture = PlayerManager.findFace(playerInput.devices[0]);
            // Texture2D faceTexture = new Texture2D(2, 2);
            // faceTexture.LoadImage(PlayerManager.playerStats[playerInput.devices[0]].face);
            // if (faceTexture == null)
            //     Debug.LogError("Texture not loaded!");
            // else
            //     Debug.Log("Texture loaded successfully.");
            renderer_face.material = new Material(renderer_face.material);
            renderer_face.material.mainTexture = faceTexture;

            playerInput.transform.SetParent(emptySlot, false);
            float offsetY = -emptySlot.GetComponent<RectTransform>().rect.height * 0.15f;
            playerInput.transform.localPosition = new Vector3(0f, offsetY, 0f);
            playerInput.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            playerInput.transform.localScale = Vector3.one * 150f;
            playerInput.DeactivateInput();
        }
    }
}