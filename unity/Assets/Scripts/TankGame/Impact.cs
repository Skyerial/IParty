using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Handles impact events in the tank game.
 * This script is responsible for detecting collisions with players bullets.
 */
public class Impact : MonoBehaviour
{
    public GameObject explosionPrefab;

    /**
     * @brief Called when a collision occurs.
     */
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit!");
            Debug.Log(collision.gameObject.name);
            PlayerInput playerInput = collision.gameObject.GetComponent<PlayerInput>();
            GameManager.PlayerDied(playerInput);
            Destroy(collision.gameObject);
        }
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
        // else if (collision.gameObject.CompareTag("Wall"))
        // {
        //     Debug.Log("Wall hit!");

        //     Rigidbody rb = GetComponent<Rigidbody>();
        //     Vector3 inDirection = rb.linearVelocity;

        //     // TEST
        //     // ContactPoint contact = collision.contacts[0];
        //     // // Vector3 normal = contact.normal;
        //     // // Vector3 reflected = Vector3.Reflect(inDirection, normal);

        //     Quaternion targetRot = Quaternion.LookRotation(inDirection.normalized);
        //     transform.rotation = targetRot;

        //     rb.constraints = RigidbodyConstraints.FreezeRotationX
        //                | RigidbodyConstraints.FreezeRotationY
        //                | RigidbodyConstraints.FreezeRotationZ;

        //     // Debug.DrawRay(contact.point, inDirection, Color.red, 1f); // Draw normal
        // }
    }
}
