// TurfUtilities.cs
using UnityEngine;

public static class TurfUtilities
{
    public static Color GetPlayerColor(Transform playerTransform)
    {
        var body = playerTransform.Find("Body.008");
        if (body == null) return Color.white;
        var rend = body.GetComponentInChildren<SkinnedMeshRenderer>();
        return rend != null ? rend.material.color : Color.white;
    }

    public static bool IsOnOwnTurf(Transform playerTransform, Color playerColor, LayerMask layerMask, float turfCheckDistance)
    {
        var origin = playerTransform.position + Vector3.up * 0.1f;
        var maxDistance = turfCheckDistance + 0.1f;
        if (Physics.Raycast(origin, Vector3.down, out var hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            var paintable = hit.collider.GetComponent<PaintableSurface>();
            if (paintable == null) return false;
            var rend = hit.collider.GetComponent<Renderer>();
            return rend != null && rend.material.color == playerColor;
        }
        return false;
    }
}
