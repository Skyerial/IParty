using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SetCard : MonoBehaviour{
    private GridScript gridScript;
    private CardData cardData;

    public Button button;
    public Image img;
    public GameObject symbolPrefab;
    public Transform symbolParent;
    public Sprite[] shapeSprites;

    public void Initialize(CardData data) {
        foreach (Transform child in symbolParent) {
            Destroy(child.gameObject);
        }

        Image cardImage = symbolParent.GetComponent<Image>();

        cardData = data;
        button.onClick.AddListener(OnCardSelect);

        int index = 3*data.number + 9*data.filling + data.shape;
        Color cardColor = GetColor(data.color);
        Sprite shapeSprite = shapeSprites[index];

        if (cardImage != null) {
            cardImage.sprite = shapeSprite;
        }

        cardImage.color = cardColor;
    }

    public void SetGridScript(GridScript script) {
        gridScript = script;
    }

    void OnCardSelect() {
        gridScript.CardSelected(cardData);
    }

    Color GetColor(int color) {
        return color switch {
            0 => new Color(1f, 0, 0),       // red
            1 => new Color(0, 1f, 0),       // green
            2 => new Color(0.5f, 0, 1f),    // purple
            _ => new Color(0, 0, 0)         // black
        };
    }
}
