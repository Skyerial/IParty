using UnityEngine;

[ExecuteInEditMode]
public class TileGroupExample : MonoBehaviour
{
    public bool autoLink = true;
    public Material blue_rock;
    public Material basic_rock;
    public Material red_rock;
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
            tileHandler tileScript = child.GetComponent<tileHandler>();
            if (tileScript.tileType == 2)
            {
                child.GetComponent<Renderer>().material = blue_rock;
            }
            else if (tileScript.tileType == 1)
            {
                child.GetComponent<Renderer>().material = red_rock;
            }
            else
            {
                child.GetComponent<Renderer>().material = basic_rock;
            }

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