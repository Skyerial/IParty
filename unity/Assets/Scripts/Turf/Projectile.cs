// Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("How often to check for ground below (seconds)")]
    public float paintCheckInterval = 0.05f;

    [Tooltip("Maximum distance to check below projectile")]
    public float maxPaintDistance = 10f;

    private LayerMask paintLayerMask;
    private Color paintColor;
    private float nextPaintTime;

    public void Initialize(LayerMask layerMask, Color color)
    {
        paintLayerMask = layerMask;
        paintColor = color;
        nextPaintTime = Time.time;
    }

    private void Update()
    {
        if (Time.time < nextPaintTime) return;
        nextPaintTime = Time.time + paintCheckInterval;
        CheckForPaintableSurface();
    }

    private void CheckForPaintableSurface()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, maxPaintDistance, paintLayerMask))
            TryPaint(hit.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        int mask = 1 << other.gameObject.layer;
        if ((paintLayerMask.value & mask) != 0)
        {
            TryPaint(other);
            Destroy(gameObject);
        }
    }

    private void TryPaint(Collider col)
    {
        var paintable = col.GetComponent<PaintableSurface>();
        if (paintable == null) return;
        paintable.PaintEntireSurface(paintColor);
    }
}
