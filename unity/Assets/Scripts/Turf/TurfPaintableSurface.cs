using UnityEngine;

public class TurfPaintableSurface : MonoBehaviour
{
    public Color CurrentColor { get; private set; }
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        CurrentColor = rend.material.color;
    }

    public void PaintEntireSurface(Color c)
    {
        CurrentColor = c;
        rend.material.color = c;
    }
}