using UnityEngine;

public class AcceleratedCameraMove : MonoBehaviour
{
    public Vector3 startPosition;     // Start (A)
    public Vector3 targetPosition;    // Target (B)
    public float maxSpeed = 5f;       // Top speed
    public float acceleration = 2f;   // Units per second²

    private float currentSpeed = 0f;
    private Vector3 direction;
    private float distanceTotal;
    private bool isMoving = true;

    void Start()
    {
        transform.position = startPosition;
        direction = (targetPosition - startPosition).normalized;
        distanceTotal = Vector3.Distance(startPosition, targetPosition);
    }

    void Update()
    {
        if (!isMoving) return;

        float distanceRemaining = Vector3.Distance(transform.position, targetPosition);

        // Calculate the distance needed to decelerate to zero speed
        float decelDistance = (currentSpeed * currentSpeed) / (2 * acceleration);

        if (distanceRemaining <= 0.01f)
        {
            transform.position = targetPosition;
            currentSpeed = 0f;
            isMoving = false;
            return;
        }

        // Decide if accelerating or decelerating
        if (decelDistance >= distanceRemaining)
        {
            // Decelerate
            currentSpeed -= acceleration * Time.deltaTime;
            if (currentSpeed < 0) currentSpeed = 0;
        }
        else
        {
            // Accelerate
            currentSpeed += acceleration * Time.deltaTime;
            if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;
        }

        // Move the camera forward
        transform.position += direction * currentSpeed * Time.deltaTime;
    }
}
