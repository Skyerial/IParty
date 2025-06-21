using UnityEngine;
using UnityEngine.InputSystem;

public class Bomb : MonoBehaviour
{
    private float countdownTime;
    private float elapsedTime;
    private float flickerTimer;
    private Renderer rend;
    private Color originalColor;
    private bool isRed = false;
    public GameObject explosion;
    public bool isBeingThrown = false;
    AudioSource audioSource;
    private bool hasPlayedThrowSound = false;

    void Start()
    {
        countdownTime = Random.Range(10f, 18f);
        rend = GetComponentInChildren<Renderer>();
        audioSource = GetComponent<AudioSource>();
        originalColor = rend.material.color;
        flickerTimer = 0f;
        elapsedTime = 0f;

    }

    void Update()
    {
        if (!isBeingThrown)
        {
            elapsedTime += Time.deltaTime;
            hasPlayedThrowSound = false;
        }
        else
        {
            audioSource.Play();
            hasPlayedThrowSound = true;

        }

        float flickerInterval = Mathf.Lerp(0.5f, 0.05f, elapsedTime / countdownTime);
        flickerTimer += Time.deltaTime;

        float scale = Mathf.Lerp(0.5f, 0.4f, Mathf.PingPong(flickerTimer * 4f, 1f));
        transform.localScale = new Vector3(scale, scale, scale);


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

        if (transform.parent != null)
        {
            var playerInput = transform.parent.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                var device = playerInput.devices[0];
                PlayerManager.instance.tempRankAdd(device);

                // if (device is VirtualController vc)
                // {

                //     var message = new 
                //     {
                //         type = "hotpotato",
                //         label = "deadupdate",
                //         playerstats = null
                //     };
                //     string clientId = vc.remoteId;
                //     Debug.Log("ClientID of the player who exploded: " + clientId);

                //     string json = "{\"type\":\"eliminated\",\"message\":\"You exploded!\"}";
                //     ServerManager.SendMessageToClient(clientId, json);
                // }
            }

            Destroy(transform.parent.gameObject);
        }

        Destroy(gameObject);
    }
}
