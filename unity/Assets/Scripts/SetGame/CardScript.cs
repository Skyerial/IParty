using UnityEngine;
using UnityEngine.UI;

public class SetCard : MonoBehaviour
{
    public GameObject symbolPrefab;
    public Transform symbolParent;

    public Sprite[] shapeSprites;

    public void Initialize(int shape, int color)
    {
        foreach (Transform child in symbolParent)
            Destroy(child.gameObject);

        Image cardImage = symbolParent.GetComponent<Image>();

        Color cardColor = GetColor(color);
        Sprite shapeSprite = shapeSprites[shape];

        if (cardImage != null)
        {
            cardImage.sprite = shapeSprite;
        }

        cardImage.color = cardColor;
    }

    Color GetColor(int index)
    {
        return index switch
        {
            0 => Color.red,
            1 => Color.green,
            2 => new Color(0.5f, 0, 1f), // purple
            _ => Color.black
        };
    }
}
