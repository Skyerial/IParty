using UnityEngine;

public class FloatCrown : MonoBehaviour
{
    public float amplitude = 0.5f;   
    public float frequency = 1f;     

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        float offset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + Vector3.up * offset;
    }
}
