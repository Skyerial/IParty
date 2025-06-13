using UnityEngine;

public class MoleHitDetector : MonoBehaviour
{
    [Header("Overlap Settings")]
    public Vector3 boxSize = new Vector3(0.2f, 0.2f, 0.2f);
    public LayerMask hammerLayer;

    private bool wasHit = false;

    void Update()
    {
        if (wasHit) return;

        Collider[] hits = Physics.OverlapBox(transform.position, boxSize * 0.5f, Quaternion.identity, hammerLayer);

        foreach (var hit in hits)
        {
            Debug.Log("🎯 Mole was hit by: " + hit.name);
            wasHit = true;
            break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
