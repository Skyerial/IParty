using UnityEngine;

public class HoldBomb : MonoBehaviour
{
    
    public bool isHoldingBomb = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform bomb = transform.Find("Bomb");
        isHoldingBomb = (bomb != null); 
        
    }
}
