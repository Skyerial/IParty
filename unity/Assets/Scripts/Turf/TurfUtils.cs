// TurfUtils.cs
using UnityEngine;

/**
 * @brief Utility methods for Turf mode, such as checking if a transform is on its own painted turf.
 */
public static class TurfUtils
{
    /**
     * @brief Determines whether the given transform is currently over turf painted in the player’s color.
     * @param t The Transform to check from (usually the player’s position).
     * @param playerColor The color assigned to the player.
     * @param paintMask LayerMask specifying which layers count as paintable turf.
     * @param turfCheckDistance Maximum distance below the transform to consider for turf detection.
     * @return True if the raycast hits a TurfPaintableSurface with a matching CurrentColor; false otherwise.
     */
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
