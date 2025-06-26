// TurfProjectile.cs
using UnityEngine;

/**
 * @brief Handles projectile behavior: periodically paints turf beneath it and on impact.
 */
public class TurfProjectile : MonoBehaviour
{
    /**
     * @brief Time interval (in seconds) between paint checks.
     */
    public float paintCheckInterval = 0.05f;
    /**
     * @brief Maximum distance downward to check for paintable surfaces.
     */
    public float maxPaintDistance   = 10f;

    private LayerMask paintMask;
    private Color paintColor;
    private float nextPaintTime;

    /**
     * @brief Initializes the projectile’s paint settings.
     * @param layerMask The LayerMask used for paintable surfaces.
     * @param color The color to apply when painting.
     */
    public void Initialize(LayerMask layerMask, Color color)
    {
        paintMask = layerMask;
        paintColor = color;
        nextPaintTime = Time.time;
    }

    /**
     * @brief Unity event called once per frame; checks downward raycasts at intervals to paint turf.
     */
    void Update()
    {
        if (Time.time < nextPaintTime) return;
        nextPaintTime = Time.time + paintCheckInterval;

        if (Physics.Raycast(transform.position, Vector3.down,
            out var hit, maxPaintDistance, paintMask))
        {
            TryPaint(hit.collider);
        }
    }

    /**
     * @brief Unity event called when the projectile enters a trigger; paints on contact and destroys the projectile.
     * @param other The Collider the projectile has entered.
     */
    void OnTriggerEnter(Collider other)
    {
        if ((paintMask.value & (1 << other.gameObject.layer)) != 0)
        {
            TryPaint(other);
            Destroy(gameObject);
        }
    }

    /**
     * @brief Attempts to paint the entire surface of a TurfPaintableSurface component on the collider.
     * @param col The Collider to attempt painting.
     */
    void TryPaint(Collider col)
    {
        var ps = col.GetComponent<TurfPaintableSurface>();
        if (ps != null) ps.PaintEntireSurface(paintColor);
    }
}
