using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Controls player interaction within the play zone by forwarding input actions to the player's weapon.
 */
public class PlayzoneController : MonoBehaviour
{
    /**
     * @brief Reference to the WeaponController component attached to the player's weapon.
     */
    private WeaponController weapon;

    /**
     * @brief Unity start callback; finds and caches the WeaponController in child objects.
     */
    void Start()
    {
        weapon = GetComponentInChildren<WeaponController>();
    }

    /**
     * @brief Called when the "Jump" input action is triggered; instructs the weapon to perform a slam.
     */
    public void OnJump()
    {
        weapon?.Slam();
    }
}
