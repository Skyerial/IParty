using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridScript : MonoBehaviour
{
    public GameObject cardPrefab;
    private List<CardData> allCards = new List<CardData>();
    private List<CardData> dealtCards = new List<CardData>();

    void Start()
    {
        CreateCards();
        Shuffle();
        DealCards();
        CreateCardGrid();
    }

    // Checks if cards c1, c2 and c3 form a valid set.
    // i.e. for all 4 attributes, it's all the same, or all different.
    bool isSet(CardData c1, CardData c2, CardData c3)
    {
        return
        (
            (
                ((c1.number == c2.number) && (c2.number == c3.number)) ||
                ((c1.number != c2.number) && (c2.number != c3.number) && (c1.number != c3.number))
            ) && (
                ((c1.filling == c2.filling) && (c2.filling == c3.filling)) ||
                ((c1.filling != c2.filling) && (c2.filling != c3.filling) && (c1.filling != c3.filling))
            ) && (
                ((c1.shape == c2.shape) && (c2.shape == c3.shape)) ||
                ((c1.shape != c2.shape) && (c2.shape != c3.shape) && (c1.shape != c3.shape))
            ) && (
                ((c1.color == c2.color) && (c2.color == c3.color)) ||
                ((c1.color != c2.color) && (c2.color != c3.color) && (c1.color != c3.color))
            )
        );
    }

    // Checks if the DealtCards list contains a valid set.
    bool hasSet()
    {
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                if (i != j)
                {
                    for (int k = 0; k < 12; k++)
                    {
                        if (i != k && j != k)
                        {
                            if (isSet(dealtCards[i], dealtCards[j], dealtCards[k]))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    // Creates all 81 cards for the game.
    void CreateCards()
    {
        allCards.Clear();
        for (int n = 0; n < 3; n++)
        {
            for (int f = 0; f < 3; f++)
            {
                for (int s = 0; s < 3; s++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        allCards.Add(new CardData(n, f, s, c));
                    }
                }
            }
        }
    }

    // Shuffles the cards in the allCards list.
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

    // Moves 12 cards from the allCards to the DealtCards list.
    // Makes sure the dealt card contain a valid set,
    // if there are still cards available.
    void DealCards()
    {
        for (int i = 0; dealtCards.Count < 12; i++) {
            if (allCards.Count <= 0)
            {
                break;
            }

            dealtCards.Add(allCards[0]);
            allCards.RemoveAt(0);
        }

        while (allCards.Count > 0 && !hasSet())
        {
            int i = Random.Range(0, allCards.Count);
            int j = Random.Range(0, dealtCards.Count);
            CardData temp = allCards[i];
            allCards[i] = dealtCards[j];
            dealtCards[j] = temp;
        }
    }

    // Initializes the 12 cards in the DealtCards list.
    void CreateCardGrid()
    {
        for (int i = 0; i < 12; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, transform);
            SetCard setCard = cardGO.GetComponent<SetCard>();
            CardData data = dealtCards[i];
            setCard.Initialize(data.number, data.filling, data.shape, data.color);
        }
    }
}

[System.Serializable]
public struct CardData
{
    public int number;
    public int filling;
    public int shape;
    public int color;

    public CardData(int number, int filling, int shape, int color)
    {
        this.number = number;
        this.filling = filling;
        this.shape = shape;
        this.color = color;
    }
}
