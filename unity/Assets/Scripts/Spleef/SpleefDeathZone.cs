// SpleefDeathZone.cs
using UnityEngine;
using UnityEngine.InputSystem;

 /**
  * @brief Detects players entering the death zone and spawns a DeathIndicator at their position.
  */
public class SpleefDeathZone : MonoBehaviour
{
     /**
      * @brief Prefab containing DeathIndicator, LineRenderer, and AudioSource components.
      */
    [Tooltip("Prefab with DeathIndicator.cs + LineRenderer + AudioSource")]
    public GameObject deathIndicatorPrefab;

     /**
      * @brief Unity event called when another collider enters this trigger; handles player elimination.
      */
    private void OnTriggerEnter(Collider other)
    {
        var pi = other.GetComponent<PlayerInput>();
        if (pi != null)
        {
            Vector3 deathPos = other.transform.position;

            if (deathIndicatorPrefab != null)
            {
                var playerColor = SpleefGameManager.Instance.GetPlayerColor(pi);

                var go = Instantiate(deathIndicatorPrefab);
                var indicator = go.GetComponent<DeathIndicator>();
                if (indicator != null)
                    indicator.ShowAt(deathPos, playerColor);
            }

            SpleefGameManager.Instance.OnPlayerEliminated(pi);
        }
    }
}
