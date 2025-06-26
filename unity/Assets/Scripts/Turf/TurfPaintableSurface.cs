using UnityEngine;

/**
 * @brief Represents a surface that can be painted a solid color, tracking its current color.
 */
public class TurfPaintableSurface : MonoBehaviour
{
    /**
     * @brief The current color of the surface.
     */
    public Color CurrentColor { get; private set; }
    private Renderer rend;

    /**
     * @brief Unity event called on Start; caches the Renderer component and initializes CurrentColor.
     */
    void Start()
    {
        rend = GetComponent<Renderer>();
        CurrentColor = rend.material.color;
    }

    /**
     * @brief Paints the entire surface with the specified color and updates CurrentColor.
     * @param c The new color to apply to the surface.
     */
    public void PaintEntireSurface(Color c)
    {
        CurrentColor = c;
        rend.material.color = c;
    }
}
