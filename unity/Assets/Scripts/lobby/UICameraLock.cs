using UnityEngine;

public class UICameraLock : MonoBehaviour
{
        public Transform canvasTransform;
        public Vector3 offset = new Vector3(0, 0, -10f);

        void LateUpdate()
        {
            transform.position = canvasTransform.position + offset;
            transform.LookAt(canvasTransform);
        }
}
