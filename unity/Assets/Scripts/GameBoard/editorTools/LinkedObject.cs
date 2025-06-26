using UnityEngine;
/**
 * @brief Allows linking tiles to one another so that gizmos can draw the path
 */

public class LinkedObject : MonoBehaviour
{
    /**
    * @brief The previous tile that this tile is linked to
    */
    public LinkedObject previous;
    /**
    * @brief The next tile that this tile is linked to
    */
    public LinkedObject next;

    /**
    * @brief This function draws the path in the editor so that someone can
    * view the path that the player will walk along.
    */
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

