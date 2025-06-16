// TurfManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TurfManager : MonoBehaviour
{
    [Header("UI Setup")]
    public GameObject    playerUIPrefab;
    public RectTransform uiParent;
    public Vector2       uiOffset = new Vector2(10f, -10f);

    private PaintableSurface[] allTiles;
    private int totalTiles;

    private class PlayerEntry
    {
        public Color color;
        public Image swatch;
        public TMP_Text percentText;
    }

    private readonly List<PlayerEntry> entries = new List<PlayerEntry>();

    private void Start()
    {
        allTiles = Object.FindObjectsByType<PaintableSurface>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        totalTiles = allTiles.Length;

        var players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log(players);
        var count = Mathf.Min(players.Length, 4);
        Debug.Log(players.Length); 

        for (var i = 0; i < count; i++)
        {
            var col = TurfUtilities.GetPlayerColor(players[i].transform);
            Debug.Log(col);

            var uiObj = Instantiate(playerUIPrefab, uiParent);
            var rect = uiObj.GetComponent<RectTransform>();

            SetAnchor(rect, i);

            var offset = uiOffset;
            if (i == 1 || i == 3) offset.x = -offset.x;
            if (i == 2 || i == 3) offset.y = -offset.y;
            rect.anchoredPosition = offset;

            var swatch = uiObj.transform.Find("Swatch").GetComponent<Image>();
            var percentText = uiObj.transform.Find("PercentText").GetComponent<TMP_Text>();
            swatch.color = col;
            percentText.text = "0.0%";

            entries.Add(new PlayerEntry { color = col, swatch = swatch, percentText = percentText });
        }
    }

    private void Update()
    {
        foreach (var e in entries)
        {
            var owned = 0;
            foreach (var tile in allTiles)
                if (tile.GetComponent<Renderer>().material.color == e.color)
                    owned++;

            var pct = totalTiles > 0 ? (owned / (float)totalTiles) * 100f : 0f;
            e.percentText.text = $"{pct:0.0}%";
        }
    }

    private void SetAnchor(RectTransform rect, int index)
    {
        switch (index)
        {
            case 0:
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                break;
            case 1:
                rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                break;
            case 2:
                rect.anchorMin = rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                break;
            case 3:
                rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                break;
        }
    }
}
