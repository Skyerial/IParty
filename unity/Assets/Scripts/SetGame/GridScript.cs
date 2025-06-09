using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridScript : MonoBehaviour
{
    public GameObject cardPrefab;
    private List<CardData> allCards = new List<CardData>();
    private int cardsToDeal = 12;

    void Start()
    {
        CreateCards();
        Shuffle();
        CreateCardGrid();
    }

    void CreateCards()
    {
        allCards.Clear();
        for (int s = 0; s < 18; s++)
        {
            for (int c = 0; c < 3; c++)
            {
                allCards.Add(new CardData(s, c));
            }
        }
    }

    void Shuffle()
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            CardData temp = allCards[i];
            int randIndex = Random.Range(i, allCards.Count);
            allCards[i] = allCards[randIndex];
            allCards[randIndex] = temp;
        }
    }

    void CreateCardGrid()
    {
        for (int i = 0; i < cardsToDeal; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, transform);
            SetCard setCard = cardGO.GetComponent<SetCard>();
            CardData data = allCards[i];
            setCard.Initialize(data.shape, data.color);
        }
    }
}

[System.Serializable]
public struct CardData
{
    public int shape;
    public int color;

    public CardData(int shape, int color)
    {
        this.shape = shape;
        this.color = color;
    }
}
