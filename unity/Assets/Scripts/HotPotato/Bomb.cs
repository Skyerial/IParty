using UnityEngine;

public class Bomb : MonoBehaviour
{
    private float countdownTime;
    private float elapsedTime;
    private float flickerTimer;
    private Renderer rend;
    private Color originalColor;
    private bool isRed = false;
    public GameObject explosion;

    void Start()
    {
        countdownTime = Random.Range(2f, 8f);
        rend = GetComponentInChildren<Renderer>();
        originalColor = rend.material.color;
        flickerTimer = 0f;
        elapsedTime = 0f;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        float flickerInterval = Mathf.Lerp(0.5f, 0.05f, elapsedTime / countdownTime);
        flickerTimer += Time.deltaTime;

        if (flickerTimer >= flickerInterval)
        {
            rend.material.color = isRed ? originalColor : Color.red;
            isRed = !isRed;
            flickerTimer = 0f;
        }

        if (elapsedTime >= countdownTime)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
