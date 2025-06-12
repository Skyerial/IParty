using UnityEngine;

public class PaintableSurface : MonoBehaviour
{
    public void PaintEntireSurface(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }
}