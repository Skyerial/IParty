using UnityEngine;

/**
 * @brief Handles dice throwing mechanics including physics and face detection.
 */

public class DiceThrow : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private Vector3[] diceFaceNormals = {
        Vector3.forward, // Face 1
        Vector3.up,      // Face 2
        Vector3.left,    // Face 3
        Vector3.right,   // Face 4
        Vector3.down,    // Face 5
        Vector3.back     // Face 6
    };

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

    private bool done = false;

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

        Vector3 force = new Vector3(0f, forceUp, 0f);
        Vector3 torque = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        _rigidbody.AddForce(force, ForceMode.Impulse);
        _rigidbody.AddTorque(torque, ForceMode.Impulse);

        throwTime = Time.time;
        done = true;
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
        if (Time.time - throwTime >= 2)
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