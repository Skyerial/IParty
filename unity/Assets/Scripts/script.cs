using UnityEngine;

[ExecuteInEditMode]
public class script : MonoBehaviour
{
    [Header("Spiral Settings")]
    public float radius = 5f;
    public float height = 5f;
    public float revolutions = 3f;
    public Vector3 TargetPos;
    public GameObject objectToClone;

    [Header("Runtime")]
    public bool regenerate = false;
    void Start()
    {
        if (transform.parent != null)
        {
            string parentName = transform.parent.name;
            Debug.Log("Parent name: " + parentName);
        }
        else
        {
            Debug.Log("This GameObject has no parent.");
        }
    }
    private void Update()
    {
        if (regenerate)
        {
            regenerate = false;


            ClearChildren();
            GenerateSpiral();

        }
    }

    void GenerateSpiral()
    {
        float b = -radius / (8 * 3.14f);
        float a = radius;
        float theta = 0;
        int step_distance = 1;

        for (int i = 0; i <= 1000; i++) {
            float r = a + b * theta;
            if (r < 0) {
                return;
            }
            float x = TargetPos.x + r * Mathf.Cos(theta);
            float dynamicY = TargetPos.y + (1 - (r / radius)) * height;
            float z = TargetPos.z + r * Mathf.Sin(theta);

            Vector3 pos = new Vector3(
                x,
                dynamicY,
                z
            );
            float dr_dtheta = b;
            float ds_dtheta = Mathf.Sqrt(dr_dtheta*dr_dtheta + r*r);

            float dtheta = step_distance / ds_dtheta;
            theta = theta + dtheta;

            GameObject clone = Instantiate(objectToClone, pos, Quaternion.identity, transform);


        }
        
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "Stump_01(Clone)")
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
