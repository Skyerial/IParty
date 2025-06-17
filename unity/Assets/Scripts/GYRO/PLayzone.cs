using UnityEngine;
using UnityEngine.InputSystem;

public class PlayzoneController : MonoBehaviour
{
    private WeaponController weapon;

    void Start()
    {
        weapon = GetComponentInChildren<WeaponController>();
    }

    public void OnJump()
    {
        weapon?.Slam();
    }

}
