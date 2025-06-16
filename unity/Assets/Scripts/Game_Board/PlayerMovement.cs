using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public int player_id = 0;
    public int current_pos = 0;
    public int increment = 0;
    private script step_spawner;
    public CharacterController characterController;
    public Vector3 TargetPos;
    
    public float radius;

    private float theta = 0;
    private float step_distance;
    private float height;
    private Vector3 new_pos;
    private Rigidbody rb;
    private Vector3 velocity;
    public float jumpSpeed = 10f;  // Speed of the jump
    public Vector3 direction;
    private bool isJumping = false;  // Flag to check if the object is jumping
    private float r_xy;
    private float sum_xz;


    private void Start() {
        step_spawner = FindAnyObjectByType<script>();
        radius = step_spawner.radius;
        step_distance = step_spawner.step_distance;
        height = step_spawner.height;
        rb = GetComponent<Rigidbody>();
        new_pos = transform.position;
    }

    private void take_step() {

        float b = -radius / (8 * 3.14f);
        float a = radius;

        float r = a + b * theta;
        if (player_id == 1) {
            r_xy = r - 0.1f;
        } else if (player_id == 2) {
            r_xy = r - 0.03f;
        } else if (player_id == 3) {
            r_xy = r + 0.03f;
        } else if (player_id == 4) {
            r_xy = r + 0.1f;
        } else {
            r_xy = r;
        }
        float x = TargetPos.x + r_xy * Mathf.Cos(theta);
        float y = TargetPos.y + (1 - (r / radius)) * height;
        float z = TargetPos.z + r_xy * Mathf.Sin(theta);
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
        transform.rotation = Quaternion.Euler(0, - (theta/(3.14f)*180f), 0);
        if (increment > 0 && isJumping == false) {
            take_step();
            //StartCoroutine(JumpToPosition());
            rb.linearVelocity = new Vector3(0,5f,0);
            isJumping = true;

        } else if (isJumping == true) {
            direction = (new_pos - transform.position);
            sum_xz = Mathf.Abs(direction.x + direction.z) * 0.3f;
            if (sum_xz < 0.1f) {
                sum_xz = 0.1f;
            }
            direction.x = direction.x / sum_xz;
            direction.y = rb.linearVelocity.y;
            direction.z = direction.z / sum_xz;
            // Calculate the velocity needed to move towards the new position
            rb.linearVelocity = direction;
            if (Vector3.Distance(transform.position, new_pos) < 0.1f) {
                isJumping = false;
            }
        } else if (isJumping == true) {
            transform.position = new_pos;
        }
    }

}
