using UnityEngine;

public class Impact : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("HIT!!");
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
        else if (collision.collider.CompareTag("Wall"))
        {
            Debug.Log("Wall hit");
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            Vector3 direction = Vector3.Reflect(rb.linearVelocity, collision.contacts[0].normal);
        }
    }
}
