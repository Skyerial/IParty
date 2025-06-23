using UnityEngine;

public class LinkedObject : MonoBehaviour
{
    public LinkedObject previous;
    public LinkedObject next;

    private void OnDrawGizmos()
    {
        if (next != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, next.transform.position);
        }

        if (previous != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, previous.transform.position);
        }
    }
}

