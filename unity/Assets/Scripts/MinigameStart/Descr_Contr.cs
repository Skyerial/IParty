using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Text;
using UnityEditor;

/**
 * @brief Controls the description and controls UI for a minigame description.
 * This script manages the display of game descriptions and controls,
 * allowing users to switch between different minigames.
 */
public class Descr_Contr : MonoBehaviour
{
    public Image targetImage; // this is the visible UI element to color
    public Image otherTargetImage;
    public TextMeshProUGUI text;
    public Image explanationImage;
    public Color selectedColor;
    public Color unselectedColor;
    public bool isSelected;
    public bool isDescriptionButton;
    public Descr_Contr otherScript;  // Reference to the *other instance* of this same script
    private string minigame = PlayerManager.currentMinigame;

    private void loadText(string minigame_name)
    {
        string fileName;
        if (isDescriptionButton)
        {
            fileName = minigame_name + "Description";
        }
        else
        {
            fileName = minigame_name + "Controls";
        }

        string filePath = Path.Combine("Game Descriptions/", minigame_name, fileName);

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

    private void SetImage(string minigame_name)
    {
        string fileName;
        if (isDescriptionButton)
        {
            fileName = minigame_name + "DescriptionImage";
        }
        else
        {
            fileName = minigame_name + "ControlsImage";
        }

        string filePath = Path.Combine("Game Descriptions/", minigame_name, fileName);
        // Load sprite from Resources/MyImages
        Sprite newSprite = Resources.Load<Sprite>(filePath);

        if (newSprite != null)
        {
            explanationImage.sprite = newSprite;
        }
        else
        {
            Debug.LogWarning("Sprite not found: " + filePath);
        }
    }

    private void Start()
    {
        if (isSelected)
        {
            loadText(minigame);
            SetImage(minigame);
        }
    }
    public void OnClick()
    {
        if (!isSelected)
        {
            isSelected = true;
            otherScript.SetSelected(false);

            targetImage.color = isSelected ? selectedColor : unselectedColor;
            otherTargetImage.color = isSelected ? unselectedColor : selectedColor;
            loadText(minigame);
            SetImage(minigame);

            Vector2 originalSize = explanationImage.rectTransform.sizeDelta;
            explanationImage.rectTransform.sizeDelta = new Vector2(originalSize.y, originalSize.x);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }
}
