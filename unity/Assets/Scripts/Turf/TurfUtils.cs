using UnityEngine;

public static class TurfUtils
{
    public static bool IsOnOwnTurf(
        Transform t, Color playerColor, LayerMask paintMask, float turfCheckDistance)
    {
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
