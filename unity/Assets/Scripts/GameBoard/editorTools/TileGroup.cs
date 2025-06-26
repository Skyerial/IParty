using UnityEngine;
/**
 * @brief handles the tile group on the game board. This means autolinking
 * child tiles to one another and assigning proper materials to tile types
 */

[ExecuteInEditMode]
public class TileGroupExample : MonoBehaviour
{
    /**
    * @brief boolean that can be switched off and on again to reload the
    * tile group (it will reassign materials)
    */
    public bool autoLink = true;

    /**
    * @brief blue rock material assigned to tiles of type 2
    */
    public Material blue_rock;

    /**
    * @brief basic rock material assigned to tiles of type 0
    */
    public Material basic_rock;

    /**
    * @brief gold rock material assigned to tiles of type 3
    */
    public Material gold_rock;

    /**
    * @brief function called upon inspector changes in edit mode. This function
    * causes tiles to be linked accordingly
    */
    private void OnValidate()
    {
        if (autoLink)
        {
            LinkTiles();
        }
    }

    /**
    * @brief links all tiles as follows. Looks at children of the tilegroup
    * gameobject and assigns the first tile's 'next' link to the second tile.
    * The second child's 'previous' will be the first tile and its 'next' will
    * be the third tile. This goes on until the last tile. The last tile is
    * only assigned a 'previous' whereas the first tile is only assigned a
    * 'next'. Next to assigning links, it also assigns material to the tiles
    * depending on the tile type.
    */
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
            else if (tileScript.tileType == 3)
            {
                child.GetComponent<Renderer>().material = gold_rock;
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