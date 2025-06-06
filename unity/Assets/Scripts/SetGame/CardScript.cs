using UnityEngine;

public class SetCard : MonoBehaviour
{
    public GameObject symbolPrefab;
    public Transform symbolParent;

    public Sprite[] shapeSprites;     // 0 = diamond, 1 = squiggle, 2 = oval
    public Material[] shadingMaterials; // 0 = solid, 1 = striped, 2 = open

    public void Initialize(int number, int shape, int color, int shading)
    {
        foreach (Transform child in symbolParent)
            Destroy(child.gameObject);

        Color cardColor = GetColor(color);
        Material shadingMat = shadingMaterials[shading];
        Sprite shapeSprite = shapeSprites[shape];

        float spacing = 0.7f;
        float startY = -(number - 1) * spacing / 2;

        for (int i = 0; i < number; i++)
        {
            GameObject sym = Instantiate(symbolPrefab, symbolParent);
            sym.transform.localPosition = new Vector3(0, startY + i * spacing, 0);
            SpriteRenderer sr = sym.GetComponent<SpriteRenderer>();
            sr.sprite = shapeSprite;
            sr.material = new Material(shadingMat);
            sr.material.color = cardColor;
        }
    }

    Color GetColor(int index)
    {
        return index switch
        {
            0 => Color.red,
            1 => Color.green,
            2 => new Color(0.5f, 0, 1f), // purple
        };
    }
}
