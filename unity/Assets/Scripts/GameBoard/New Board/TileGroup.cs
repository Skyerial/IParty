using UnityEngine;

[ExecuteInEditMode]
public class TileGroupExample : MonoBehaviour
{
    public bool autoLink = true;
    private void OnValidate()
    {
        if (autoLink)
        {
            LinkTiles();
        }
    }
    private void LinkTiles()
    {
        // Get all immediate children of this GameObject (TileGroup)
        LinkedObject previous = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Debug.Log($"Child {i}: {child.name}");

            // You can also access components on the child, e.g.:
            LinkedObject link = child.GetComponent<LinkedObject>();
            if (link != null)
            {
                link.previous = previous;
                if (previous != null)
                {
                    previous.next = link;
                }
                previous = link;
            }
        }
    }
}