// TurfPlayerShooter.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
/**
 * @brief Handles firing paint projectiles with spread, managing ammo, fire rate, and turf-based regeneration penalties.
 */
public class TurfPlayerShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    /**
     * @brief Prefab for the projectile to be fired.
     */
    public GameObject projectilePrefab;
    /**
     * @brief Transform representing the spawn point for projectiles.
     */
    public Transform firePoint;
    /**
     * @brief Speed at which the projectile travels.
     */
    public float projectileSpeed = 20f;
    /**
     * @brief Minimum time interval between successive shots.
     */
    public float fireRate = 0.2f;
    /**
     * @brief Maximum random angle offset applied to projectile direction.
     */
    public float maxSpreadAngle = 5f;
    /**
     * @brief Lifetime of the spawned projectile before automatic destruction.
     */
    public float projectileLifeTime = 5f;
    /**
     * @brief Layer mask used by projectiles to paint turf.
     */
    public LayerMask paintLayerMask;
    /**
     * @brief Distance to check for turf beneath the player for regeneration penalty.
     */
    public float turfCheckDistance = 1f;
    /**
     * @brief Sound effect played when shooting.
     */
    public AudioClip shootSound;

    [Header("Ammo UI")]
    /**
     * @brief Maximum ammo capacity.
     */
    public int maxAmmo = 10;
    /**
     * @brief Rate at which ammo regenerates per second on own turf.
     */
    public float ammoRegenRate = 1f;
    /**
     * @brief Regeneration multiplier when off own turf.
     */
    public float turfRegenPenalty = 0.5f;
    /**
     * @brief UI Image used to display current ammo level.
     */
    public Image ammoBarFill;

    private PlayerInput pi;
    private InputAction attackAction;
    private AudioSource audioSrc;
    private float currentAmmo;
    private float nextFireTime;
    private bool isFiring;
    private Color playerColor;

    /**
     * @brief Unity event called when the script instance is loaded; sets up input actions for firing.
     */
    void Awake()
    {
        pi = GetComponent<PlayerInput>();
        attackAction = pi.actions["Attack"];
        attackAction.performed += _ => isFiring = true;
        attackAction.canceled += _ => isFiring = false;
    }

    /**
     * @brief Unity event called when the object becomes active; enables the attack input action.
     */
    void OnEnable()
    {
        attackAction.Enable();
    }

    /**
     * @brief Unity event called when the object becomes inactive; disables the attack input action.
     */
    void OnDisable()
    {
        attackAction.Disable();
    }

    /**
     * @brief Unity event called on the first frame; initializes ammo, UI, retrieves player color, and prepares audio source.
     */
    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        playerColor = TurfGameManager.Instance.GetPlayerColor(pi);

        audioSrc = GetComponent<AudioSource>()
                   ?? (shootSound != null
                       ? gameObject.AddComponent<AudioSource>()
                       : null);
    }

    /**
     * @brief Unity event called once per frame; handles firing logic and ammo regeneration with turf penalties.
     */
    void Update()
    {
        if (isFiring && Time.time >= nextFireTime)
        {
            if (currentAmmo >= 1f)
            {
                Shoot();
                currentAmmo -= 1f;
                UpdateAmmoUI();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                isFiring = false;
            }
        }
        else if (!isFiring && currentAmmo < maxAmmo)
        {
            bool onOwn = TurfUtils.IsOnOwnTurf(
                transform, playerColor, paintLayerMask, turfCheckDistance
            );
            float regenMul = onOwn ? 1f : turfRegenPenalty;

            currentAmmo = Mathf.Min(
                maxAmmo,
                currentAmmo + ammoRegenRate * regenMul * Time.deltaTime
            );
            UpdateAmmoUI();
        }
    }

    /**
     * @brief Spawns a projectile with optional spread, sets its velocity and paint behavior, and plays shooting sound.
     */
    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        var rot = firePoint.rotation;
        if (maxSpreadAngle > 0f)
            rot = Quaternion.RotateTowards(
                rot, Random.rotation, Random.Range(0f, maxSpreadAngle)
            );

        var projGO = Instantiate(projectilePrefab, firePoint.position, rot);
        Destroy(projGO, projectileLifeTime);

        var tp = projGO.GetComponent<TurfProjectile>();
        tp.Initialize(paintLayerMask, playerColor);

        if (projGO.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = rot * Vector3.forward * projectileSpeed;

        if (shootSound != null && audioSrc != null)
            audioSrc.PlayOneShot(shootSound);
    }

    /**
     * @brief Updates the ammo UI fill amount based on current ammunition.
     */
    private void UpdateAmmoUI()
    {
        if (ammoBarFill != null)
            ammoBarFill.fillAmount = currentAmmo / maxAmmo;
    }
}

