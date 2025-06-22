using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class Reconnect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject ReconnectPanel;
    public TMP_Text Name;
    public RawImage face;
    public TMP_Text reconnectCode;
    private string connectionId;

    public void DisconnectEvent(string id, string code)
    {
        connectionId = id;
        ReconnectPanel.SetActive(true);
        Time.timeScale = 0;

        var device = ServerManager.allControllers[id];
        var name = PlayerManager.playerStats[device].name;
        Texture2D faceTexture = PlayerManager.findFace(device);
        Name.text = name;
        face.texture = faceTexture;
        reconnectCode.text = code;
    }

    public void ReconnectEvent()
    {
        ReconnectPanel.SetActive(false);
        Time.timeScale = 1;
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


        ReconnectPanel.SetActive(false);
        Time.timeScale = 1;
    }
}
