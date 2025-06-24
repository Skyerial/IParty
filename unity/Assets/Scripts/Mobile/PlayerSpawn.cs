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
    public GameObject nameboardPrefab;
    int i = 0;

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Lobby") ServerManager.allControllers?.Clear();
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject spawnParent = GameObject.Find("Spawn");

        Transform[] spawnPoints = spawnParent.GetComponentsInChildren<Transform>()
                                            .Where(t => t != spawnParent.transform)
                                            .ToArray();

        int spawnIndex = i % spawnPoints.Length;
        playerInput.transform.position = spawnPoints[spawnIndex].position;
        playerInput.transform.rotation = spawnPoints[spawnIndex].rotation;
        Debug.Log($"Spawning player at: {spawnPoints[spawnIndex].name}");
        i++;

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Lobby")
        {
            Transform body = playerInput.transform.Find("Body");
            if (body != null && body.TryGetComponent(out SkinnedMeshRenderer renderer))
            {
                renderer.material = PlayerManager.findColor(playerInput.devices[0]);
            }

            Transform face = playerInput.transform.Find("Face");
            if (face != null && face.TryGetComponent(out SkinnedMeshRenderer renderer_face))
            {
                Material newMat = new Material(renderer_face.material);
                InputDevice device = playerInput.devices.Count > 0 ? playerInput.devices[0] : null;
                Texture2D faceTexture = (device != null) ? PlayerManager.findFace(device) : null;

                if (faceTexture != null && faceTexture.width > 2)
                {
                    newMat.mainTexture = faceTexture;
                }
                else
                {
                    newMat.mainTexture = Texture2D.blackTexture;
                }

                renderer_face.material = newMat;
            }

            if (nameboardPrefab != null)
            {
                GameObject nameboard = Instantiate(nameboardPrefab, playerInput.transform);
                nameboard.transform.localPosition = new Vector3(0, 2f, 0);
                nameboard.transform.localRotation = Quaternion.identity;

                TextMeshProUGUI text = nameboard.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = PlayerManager.playerStats[playerInput.devices[0]].name;
                }
            }

            playerInput.transform.localScale = Vector3.one * 2f;
            playerInput.DeactivateInput();
        }
    }
}
