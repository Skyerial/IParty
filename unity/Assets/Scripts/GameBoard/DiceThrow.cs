using UnityEngine;

public class DiceThrow : MonoBehaviour
{
    private Rigidbody _rigidbody;

    // Define face normal vectors instead of rotations
    // These represent the "up" direction for each face when that face is on top
    private Vector3[] diceFaceNormals = {
        Vector3.forward, // Face 1
        Vector3.up,      // Face 2 (top face in default orientation)
        Vector3.left,    // Face 3
        Vector3.right,   // Face 4
        Vector3.down,    // Face 5 (bottom face)
        Vector3.back     // Face 6
    };

    [SerializeField]
    public float forceUp = 2f; // Force applied to the dice when thrown
    public float velocityThreshold = 0.01f; // Velocity below this = dice settled
    public float maxWaitTime = 5f; // Maximum time to wait for dice to settle
    public float heightAbovePlayer = 2f;
    public bool debug = false;
    private bool done = false;

    private float throwTime; // Time when the dice was thrown

    private void OnEnable()
    {
        // Reset throw time when the script is enabled
        throwTime = Time.time;
    }

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!debug)
        {
            return; // Exit if debug mode is off
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

    public void ThrowDice()
    {
        _rigidbody.isKinematic = false;

        Vector3 force = new Vector3(0f, forceUp, 0f);
        Vector3 torque = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        _rigidbody.AddForce(force, ForceMode.Impulse);
        _rigidbody.AddTorque(torque, ForceMode.Impulse);

        throwTime = Time.time; // Update throw time when dice is thrown
        done = true;
    }

    public int SideUp()
    {
        Vector3 worldUp = Vector3.up;
        float maxDot = -1f;
        int bestFaceIndex = 0;

        for (int i = 0; i < diceFaceNormals.Length; i++)
        {
            // Transform the face normal from local space to world space
            Vector3 worldFaceNormal = _rigidbody.transform.TransformDirection(diceFaceNormals[i]);
            float dot = Vector3.Dot(worldUp, worldFaceNormal);

            if (dot > maxDot)
            {
                maxDot = dot;
                bestFaceIndex = i;
            }
        }

        Debug.Log($"Face {bestFaceIndex + 1} is up (dot product: {maxDot:F3})");
        return bestFaceIndex + 1; // Return face number (1-6)

    }

    public bool DiceSettled()
    {
        // Waiting 2 seconds before checking if the dice is set.
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
