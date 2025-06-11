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
        if (Time.time >= nextPaintTime)
        {
            CheckForPaintableSurfaces();
            nextPaintTime = Time.time + paintCheckInterval;
        }
    }

    private void CheckForPaintableSurfaces()
    {
        RaycastHit hit;
        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            maxPaintDistance,
            paintLayerMask))
        {
            PaintableSurface paintable = hit.collider.GetComponent<PaintableSurface>();
            if (paintable != null)
            {
                paintable.PaintEntireSurface(paintColor);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only react if this collider is on the paintable layer
        int otherMask = 1 << other.gameObject.layer;
        if ((paintLayerMask.value & otherMask) != 0)
        {
            var paintable = other.GetComponent<PaintableSurface>();
            if (paintable != null)
            {
                paintable.PaintEntireSurface(paintColor);
                // now destroy, but only for paint hits
                Destroy(gameObject);
            }
        }
    }
}