using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public int current_pos = 0;
    public int increment = 0;
    public script step_spawner;
    public CharacterController characterController;
    public Vector3 TargetPos;
    
    public float radius;

    public float theta = 0;
    public float step_distance;
    public float height;
    public int step_counter = 0;
    public Vector3 new_pos;
    private Rigidbody rb;
    public Vector3 velocity;
    public float jumpSpeed = 10f;  // Speed of the jump
    public Vector3 direction;
    private bool isJumping = false;  // Flag to check if the object is jumping


    private void Start(){
        float radius = step_spawner.radius;
        rb = GetComponent<Rigidbody>();
    }

    private void take_step() {
        float b = -radius / (8 * 3.14f);
        float a = radius;

        float r = a + b * theta;
        float x = TargetPos.x + r * Mathf.Cos(theta);
        float y = TargetPos.y + (1 - (r / radius)) * height;
        float z = TargetPos.z + r * Mathf.Sin(theta);
        new_pos = new Vector3(
            x,
            y,
            z
        );
        
        theta = theta + step_distance / (Mathf.Sqrt(b*b + r*r));
        increment = increment - 1;
        current_pos = current_pos + 1;

    }

    void Update()
    {
        // Trigger jump when space is pressed
        if (increment > 0 && isJumping == false) {
            take_step();
            StartCoroutine(JumpToPosition());
            transform.rotation = Quaternion.Euler(0, - (theta/(3.14f)*180f), 0);
        } if (isJumping == false) {
            transform.position = new_pos;
        }
    }

    private IEnumerator JumpToPosition()
    {
        isJumping = true;

        // While the object is not close enough to the new position
        while (Vector3.Distance(transform.position, new_pos) > 0.1f)
        {
            direction = (new_pos - transform.position);
            // Calculate the velocity needed to move towards the new position
            rb.linearVelocity = direction * jumpSpeed;

            yield return null;  // Wait for the next frame
        }

        // Ensure the object ends up exactly at the target position
        //rb.linearVelocity = Vector3.zero;  // Stop the movement once we're close enough

        isJumping = false;
    }
}
