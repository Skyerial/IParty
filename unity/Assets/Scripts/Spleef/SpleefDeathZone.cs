// SpleefDeathZone.cs (was DeathZone.cs)
using UnityEngine;
using UnityEngine.InputSystem;

public class SpleefDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var pi = other.GetComponent<PlayerInput>();
        if (pi != null)
            SpleefGameManager.Instance.OnPlayerEliminated(pi);
    }
}
