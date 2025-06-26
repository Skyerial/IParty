using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Text;
using UnityEditor;

public class LoadText : MonoBehaviour
{
    public TextMeshProUGUI text;

    private string minigame = PlayerManager.currentMinigame;

    void Start()
    {
        string fileName = minigame + "Description";
        string filePath = Path.Combine("Game Descriptions/", minigame,  fileName);

        TextAsset mytxtData = Resources.Load<TextAsset>(filePath);
        if (mytxtData != null)
        {
            text.text = mytxtData.text;
        }
        else
        {
            Debug.LogWarning("TextAsset not found at: " + filePath);
        }
    }
}
