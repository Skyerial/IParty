using UnityEngine;

public class Impact : MonoBehaviour
{
    public GameObject bulletPrefab;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit!");
            Debug.Log(collision.gameObject.name);
            GameManager.PlayerDied(collision.gameObject);
            
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall hit!");

            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 inDirection = rb.linearVelocity;

            // TEST
            // ContactPoint contact = collision.contacts[0];
            // // Vector3 normal = contact.normal;
            // // Vector3 reflected = Vector3.Reflect(inDirection, normal);

            Quaternion targetRot = Quaternion.LookRotation(inDirection.normalized);
            transform.rotation = targetRot;

            rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;
                       
            // Debug.DrawRay(contact.point, inDirection, Color.red, 1f); // Draw normal
        }
    }
}
