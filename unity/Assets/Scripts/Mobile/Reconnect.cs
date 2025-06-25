using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class Reconnect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject ReconnectPanel;
    public TMP_Text Name;
    public RawImage face;
    public TMP_Text reconnectCode;
    public Queue<(string, string)> disconnectedPlayers = new Queue<(string, string)>();
    public string connectionId;

    public void DisconnectEvent(string id, string code)
    {
        Debug.Log("Current disconnected players: " + disconnectedPlayers.Count);
        if (!ReconnectPanel.activeSelf)
        {
            ReconnectPanel.SetActive(true);
            Time.timeScale = 0;
            connectionId = id;
            MessageUpdate(id, code);
        }
        else
        {
            Debug.Log("QUEING");
            disconnectedPlayers.Enqueue((id, code));
        }
    }

    public void ReconnectEvent()
    {   
        if (disconnectedPlayers.Count >= 1)
        {
            // Removing the handled client
            // disconnectedPlayers.Dequeue();
            foreach (var (player, abc) in disconnectedPlayers)
            {
                Debug.Log(player);
            }
            var (id, code) = disconnectedPlayers.Dequeue();
            connectionId = id;
            MessageUpdate(id, code);
        }
        else
        {
            ReconnectPanel.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void Continue()
    {
        if (ServerManager.allControllers.TryGetValue(connectionId, out var dev))
        {
            foreach (var p in PlayerInput.all)
            {
                if (p.devices.Contains(dev)) { Destroy(p.gameObject); break; }
            }
            PlayerManager.RemovePlayer(ServerManager.allControllers[connectionId]);
            ServerManager.allSockets.Remove(ServerManager.allControllers[connectionId]);
            ServerManager.allControllers.Remove(connectionId);
        }


        if (disconnectedPlayers.Count >= 1)
        {
            // Removing the handled client
            // disconnectedPlayers.Dequeue();

            var (id, code) = disconnectedPlayers.Dequeue();
            connectionId = id;
            MessageUpdate(id, code);
        }
        else
        {
            ReconnectPanel.SetActive(false);
            Time.timeScale = 1;
        }
    }

    void MessageUpdate(string id, string code)
    {
        var device = ServerManager.allControllers[id];
        var name = PlayerManager.playerStats[device].name;
        Texture2D faceTexture = PlayerManager.findFace(device);
        Name.text = name;
        face.texture = faceTexture;
        reconnectCode.text = code;
    } 
}
