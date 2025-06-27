using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public Color newColor = Color.red;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = newColor;
    }
}