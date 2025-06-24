// SpleefDeathZone.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class SpleefDeathZone : MonoBehaviour
{
    [Tooltip("Prefab with DeathIndicator.cs + LineRenderer + AudioSource")]
    public GameObject deathIndicatorPrefab;

    private void OnTriggerEnter(Collider other)
    {
        var pi = other.GetComponent<PlayerInput>();
        if (pi != null)
        {
            Vector3 deathPos = other.transform.position;

            if (deathIndicatorPrefab != null)
            {
                var go = Instantiate(deathIndicatorPrefab);
                var indicator = go.GetComponent<DeathIndicator>();
                if (indicator != null)
                    indicator.ShowAt(deathPos);
            }

            SpleefGameManager.Instance.OnPlayerEliminated(pi);
        }
    }
}
