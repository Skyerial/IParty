using UnityEngine;

public class TurfProjectile : MonoBehaviour
{
    public float paintCheckInterval = 0.05f;
    public float maxPaintDistance   = 10f;

    private LayerMask paintMask;
    private Color paintColor;
    private float nextPaintTime;

    public void Initialize(LayerMask layerMask, Color color)
    {
        paintMask = layerMask;
        paintColor = color;
        nextPaintTime = Time.time;
    }

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

    void OnTriggerEnter(Collider other)
    {
        if ((paintMask.value & (1 << other.gameObject.layer)) != 0)
        {
            TryPaint(other);
            Destroy(gameObject);
        }
    }

    void TryPaint(Collider col)
    {
        var ps = col.GetComponent<TurfPaintableSurface>();
        if (ps != null) ps.PaintEntireSurface(paintColor);
    }
}
