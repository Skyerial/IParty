using UnityEngine;

/**
 * @brief Class used to change color of character sprite
 */
public class ColorChanger : MonoBehaviour
{
    public Color newColor = Color.red;

    /**
    * @brief Start function called uppon script initiation
    * handling the new color assigning of the character
    */
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = newColor;
    }
}