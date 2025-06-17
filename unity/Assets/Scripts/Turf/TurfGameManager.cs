// TurfGameManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class TurfGameManager : MonoBehaviour
{
    public static TurfGameManager Instance { get; private set; }

    [Header("UI Setup")]
    public GameObject    playerUIPrefab;   // prefab with “Swatch” (Image) & “PercentText” (TMP_Text)
    public RectTransform uiParent;         // canvas panel parent
    public Vector2       uiOffset = new Vector2(10f, -10f);

    class PlayerEntry
    {
        public InputDevice device;
        public Color       turfColor;    // cached once
        public Image       swatch;
        public TMP_Text    percentText;
    }

    readonly List<PlayerEntry>  entries    = new();
    TurfPaintableSurface[]      allTiles   = null;
    int                         totalTiles = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        // cache all paintable tiles (exclude inactive)
        allTiles = Object.FindObjectsByType<TurfPaintableSurface>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        totalTiles = allTiles.Length;
    }

    public static void RegisterPlayerGame(PlayerInput pi)
    {
        var dev = pi.devices[0];

        // load the material once and get its color
        var mat  = PlayerManager.findColor(dev);
        var col  = mat.color;

        // apply it to the "Body.008" mesh
        var body = pi.GetComponentsInChildren<SkinnedMeshRenderer>()
                     .First(r => r.name == "Body.008");
        body.material = mat;

        // build UI entry & cache color
        Instance.OnPlayerJoined(dev, col);
    }

    void OnPlayerJoined(InputDevice dev, Color turfColor)
    {
        // instantiate UI prefab
        var uiObj = Instantiate(playerUIPrefab, uiParent);
        var rect  = uiObj.GetComponent<RectTransform>();
        int idx   = entries.Count;
        SetAnchor(rect, idx);

        // position in one of four corners
        Vector2 pos = uiOffset;
        if ((idx & 1) != 0) pos.x = -pos.x;
        if ((idx & 2) != 0) pos.y = -pos.y;
        rect.anchoredPosition = pos;

        // set swatch color & initial text
        var sw  = uiObj.transform.Find("Swatch").GetComponent<Image>();
        var txt = uiObj.transform.Find("PercentText").GetComponent<TMP_Text>();
        sw.color = turfColor;
        txt.text  = "0.0%";

        // cache for Update() and FinalizeRanking()
        entries.Add(new PlayerEntry {
            device      = dev,
            turfColor   = turfColor,
            swatch      = sw,
            percentText = txt
        });
    }

    void Update()
    {
        // update turf coverage % using cached turfColor
        foreach (var e in entries)
        {
            int owned = allTiles.Count(t => t.CurrentColor == e.turfColor);
            float pct = totalTiles > 0 ? (owned / (float)totalTiles) * 100f : 0f;
            e.percentText.text = $"{pct:0.0}%";
        }
    }

    void SetAnchor(RectTransform rt, int idx)
    {
        switch (idx)
        {
            case 0:
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                break;
            case 1:
                rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                break;
            case 2:
                rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                break;
            default: // idx == 3
                rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                break;
        }
    }

    public void FinalizeRanking()
    {
        var pm = Object.FindFirstObjectByType<PlayerManager>();
        pm.tempRankClear();

        // sort devices by cached turfColor coverage
        var sorted = entries
            .OrderBy(e => allTiles.Count(t => t.CurrentColor == e.turfColor))
            .Select(e => e.device);

        foreach (var dev in sorted)
            pm.tempRankAdd(dev);
    }
}
