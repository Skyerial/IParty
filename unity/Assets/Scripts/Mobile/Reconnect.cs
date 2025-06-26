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
    // public Queue<(string, string)> disconnectedPlayers = new Queue<(string, string)>();
    public bool disconnected;

    void Awake()
    {
        disconnected = false;
        DontDestroyOnLoad(gameObject);
    }

    public void DisconnectEvent(string id, string code)
    {
        disconnected = true;
        ReconnectPanel.SetActive(true);
        Time.timeScale = 0;
        MessageUpdate(id, code);
    }

    public void ReconnectEvent()
    {
        ReconnectPanel.SetActive(false);
        Time.timeScale = 1;
        disconnected = false;
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
