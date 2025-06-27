using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Text;
using UnityEditor;

/**
 * @brief Loads and displays the description text for a minigame description.
 * This script retrieves the description text from a TextAsset based on the current minigame
 * and displays it in a TextMeshProUGUI component.
 */
public class LoadText : MonoBehaviour
{
    public TextMeshProUGUI text;

    private string minigame = PlayerManager.currentMinigame;

    /**
     * @brief Start is called before the first frame update.
     * This method loads the description text for the current minigame
     * from a TextAsset and sets it to the TextMeshProUGUI component.
     */
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
