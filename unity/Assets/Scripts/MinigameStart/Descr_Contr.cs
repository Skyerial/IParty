using UnityEngine;
public class Descr_Contr : MonoBehaviour
{
    int clickCount = 0;

    public void OnClick()
    {
        clickCount++;
        Debug.Log("Clicked " + clickCount + " times");
    }
}

