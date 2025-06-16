using UnityEngine;
public class Descr_Contr : MonoBehaviour
{
    int clickCount = 0;

    public void goofyName()
    {
        clickCount++;
        Debug.Log("Clicked " + clickCount + " times");
    }
}

