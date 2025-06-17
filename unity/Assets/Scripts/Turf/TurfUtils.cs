using UnityEngine;

public static class TurfUtils
{
    /// <summary>
    /// Returns true if the surface directly below 't' (within distance + 0.1f),
    /// when cast from 0.1m above, matches the playerColor.
    /// </summary>
    public static bool IsOnOwnTurf(
        Transform t, Color playerColor, LayerMask paintMask, float turfCheckDistance)
    {
        // match original origin & maxDistance :contentReference[oaicite:8]{index=8}
        Vector3 origin = t.position + Vector3.up * 0.1f;
        float maxDistance = turfCheckDistance + 0.1f;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out var hit,
            maxDistance,
            paintMask,
            QueryTriggerInteraction.Ignore))
        {
            var ps = hit.collider.GetComponent<TurfPaintableSurface>();
            return ps != null && ps.CurrentColor == playerColor;
        }
        return false;
    }
}
