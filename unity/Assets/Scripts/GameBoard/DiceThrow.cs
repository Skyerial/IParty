using UnityEngine;

/**
 * @brief Handles dice throwing mechanics including physics and face detection.
 */
public class DiceThrow : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 startPosition;
    private bool _firstThrow = true;

    private Vector3[] diceFaceNormals = {
        Vector3.forward, // Face 1
        Vector3.up,      // Face 2
        Vector3.left,    // Face 3
        Vector3.right,   // Face 4
        Vector3.down,    // Face 5
        Vector3.back     // Face 6
    };

    [SerializeField]
    [Tooltip("Horizontal force magnitude on the XZ plane.")]
    public float horizontalForce = 5f;

    [SerializeField]
    public float forceUp = 2f;

    /**
     * @brief Threshold velocity below which the dice is considered settled.
     */
    public float velocityThreshold = 0.01f;

    /**
     * @brief Maximum time to wait for the dice to settle before assuming a result.
     */
    public float maxWaitTime = 5f;

    /**
     * @brief Height above the player where the dice spawns or activates.
     */
    public float heightAbovePlayer = 2f;

    /**
     * @brief Enables debug mode to allow manual dice throws via keyboard input.
     */
    public bool debug = false;

    private float throwTime;

    /**
     * @brief Unity event called when the script is enabled.
     * Resets the throw timer.
     */
    private void OnEnable()
    {
        throwTime = Time.time;
    }

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        // Record the starting position for aiming
        startPosition = transform.position;
    }

    /**
     * @brief Unity event called once per frame.
     * Allows debug input for manually throwing the dice or checking the upward face.
     */
    void Update()
    {
        if (!debug)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowDice();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SideUp();
        }
    }

    /**
     * @brief Applies force and torque to simulate throwing the dice.
     * Records throw time for settled state check.
     */
    public void ThrowDice()
    {
        _rigidbody.isKinematic = false;

        Vector3 horizontalDir;
        if (_firstThrow)
        {
            // Random horizontal direction on XZ plane
            Vector2 rand2D = Random.insideUnitCircle.normalized;
            horizontalDir = new Vector3(rand2D.x, 0f, rand2D.y);
            _firstThrow = false;
        }
        else
        {
            // Aim back at the recorded start position on the horizontal plane
            Vector3 toTarget = startPosition - transform.position;
            toTarget.y = 0f;
            horizontalDir = toTarget.normalized;
            // Add some horizontal noise
            Vector3 noise = new Vector3(
                Random.Range(-0.2f, 0.2f),
                0f,
                Random.Range(-0.2f, 0.2f)
            );
            horizontalDir = (horizontalDir + noise).normalized;
        }

        // Compose total force with adjustable upward component
        Vector3 force = horizontalDir * horizontalForce + Vector3.up * forceUp;
        _rigidbody.AddForce(force, ForceMode.Impulse);

        // Apply random spin around all three axes for realistic tumbling
        Vector3 torque = new Vector3(
            Random.Range(-10f, 10f),
            Random.Range(-10f, 10f),
            Random.Range(-10f, 10f)
        );
        _rigidbody.AddTorque(torque, ForceMode.Impulse);

        throwTime = Time.time;
    }

    /**
     * @brief Determines which face of the dice is currently facing up.
     * @return The face number (1 to 6) that is currently up.
     */
    public int SideUp()
    {
        Vector3 worldUp = Vector3.up;
        float maxDot = -1f;
        int bestFaceIndex = 0;

        for (int i = 0; i < diceFaceNormals.Length; i++)
        {
            Vector3 worldFaceNormal = _rigidbody.transform.TransformDirection(diceFaceNormals[i]);
            float dot = Vector3.Dot(worldUp, worldFaceNormal);

            if (dot > maxDot)
            {
                maxDot = dot;
                bestFaceIndex = i;
            }
        }

        Debug.Log($"Face {bestFaceIndex + 1} is up (dot product: {maxDot:F3})");
        return bestFaceIndex + 1;
    }

    /**
     * @brief Checks whether the dice has settled (stopped moving or timeout).
     * @return True if the dice is considered settled, false otherwise.
     */
    public bool DiceSettled()
    {
        Debug.Log("Current remaining: " + (Time.time - throwTime));
        if (Time.time - throwTime >= 2f)
        {
            float timeSinceThrow = Time.time - throwTime;
            float currentVelocity = _rigidbody.linearVelocity.magnitude;

            return timeSinceThrow >= maxWaitTime || currentVelocity < velocityThreshold;
        }
        else
        {
            return false;
        }
    }
}
