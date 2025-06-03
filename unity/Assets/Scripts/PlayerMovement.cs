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

    private void Start(){
        float radius = step_spawner.radius;
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
        if (increment > 0) {
            take_step();
            step_counter = 0;
        }
        step_counter = step_counter + 1;
        if (step_counter > 100) {
            transform.position = new_pos;
        }
        
        characterController.Move((new_pos - transform.position) * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, theta - 90f, 0);
    }
}
