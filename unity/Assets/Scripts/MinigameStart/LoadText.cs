using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Text;
using UnityEditor;

public class LoadText : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Start()
    {
        string fileName = "TankGame";
        string filePath = Path.Combine("Game Descriptions/", fileName);

        TextAsset mytxtData = Resources.Load<TextAsset>(filePath);
        if (mytxtData != null)
        {
            text.text = mytxtData.text;
        }
        else
        {
            Debug.LogError("TextAsset not found at: " + filePath);
        }
    }
}
