using System.Collections.Generic;
using UnityEngine;

public class SetGameManager : MonoBehaviour
{
    public GameObject cardPrefab;  // Assign your SetCardPrefab here

    private List<SetCardData> deck = new List<SetCardData>();
    private int cardsToDeal = 12;

    void Start()
    {
        GenerateDeck();
        ShuffleDeck();
        DealInitialCards();
    }

    void GenerateDeck()
    {
        deck.Clear();

        for (int n = 1; n <= 3; n++)         // Number: 1–3
        for (int s = 0; s < 3; s++)          // Shape: 0–2
        for (int c = 0; c < 3; c++)          // Color: 0–2
        for (int sh = 0; sh < 3; sh++)       // Shading: 0–2
        {
            deck.Add(new SetCardData(n, s, c, sh));
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rnd = Random.Range(i, deck.Count);
            var temp = deck[i];
            deck[i] = deck[rnd];
            deck[rnd] = temp;
        }
    }

    void DealInitialCards()
    {
        float spacingX = 2.0f;
        float spacingZ = 2.5f;
        int cols = 4;

        for (int i = 0; i < cardsToDeal; i++)
        {
            Vector3 position = new Vector3(
                (i % cols) * spacingX,
                0,
                -(i / cols) * spacingZ
            );

            GameObject cardGO = Instantiate(cardPrefab, position, Quaternion.Euler(90, 0, 0));
            SetCard card = cardGO.GetComponent<SetCard>();
            SetCardData data = deck[i];

            card.Initialize(data.Number, data.Shape, data.Color, data.Shading);
        }
    }

    struct SetCardData
    {
        public int Number, Shape, Color, Shading;

        public SetCardData(int number, int shape, int color, int shading)
        {
            Number = number;
            Shape = shape;
            Color = color;
            Shading = shading;
        }
    }

    void Update ()
    {

    }
}
