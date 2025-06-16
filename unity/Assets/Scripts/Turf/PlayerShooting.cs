// PlayerShooting.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 20f;
    public float fireRate = 0.2f;
    public float maxSpreadAngle = 5f;
    public LayerMask paintLayerMask;
    public float projectileLifeTime = 5f;

    [Header("Ammo Settings")]
    public int maxAmmo = 10;
    public float ammoRegenRate = 1f;
    public Image ammoBarFill;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioClip shootSound;

    [Header("Turf Penalty Settings")]
    public float turfCheckDistance = 1f;
    public float turfRegenPenalty = 0.5f;

    private PlayerInput playerInput;
    private InputAction shootAction;
    private AudioSource audioSource;
    private bool isFiring;
    private float nextFireTime;
    private float currentAmmo;
    private Color playerColor;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions.FindAction("Attack");

        audioSource = GetComponent<AudioSource>() ?? (shootSound != null ? gameObject.AddComponent<AudioSource>() : null);
        playerColor = TurfUtilities.GetPlayerColor(transform);

        shootAction.performed += _ => isFiring = true;
        shootAction.canceled  += _ => isFiring = false;
    }

    private void OnEnable()  => shootAction.Enable();
    private void OnDisable() => shootAction.Disable();

    private void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    private void Update()
    {
        // Ammo regen
        var regenMul = TurfUtilities.IsOnOwnTurf(transform, playerColor, paintLayerMask, turfCheckDistance)
                      ? 1f
                      : turfRegenPenalty;
        if (currentAmmo < maxAmmo)
        {
            currentAmmo = Mathf.Min(maxAmmo, currentAmmo + ammoRegenRate * regenMul * Time.deltaTime);
            UpdateAmmoUI();
        }

        // Firing
        if (isFiring && Time.time >= nextFireTime)
        {
            if (currentAmmo >= 1f)
            {
                Shoot();
                currentAmmo -= 1f;
                UpdateAmmoUI();
                nextFireTime = Time.time + fireRate;
            }
            else isFiring = false;
        }
    }

    private void UpdateAmmoUI()
    {
        if (ammoBarFill != null)
            ammoBarFill.fillAmount = currentAmmo / maxAmmo;
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Shooting parameters not set up!");
            return;
        }

        var rot = firePoint.rotation;
        if (maxSpreadAngle > 0f)
            rot = Quaternion.RotateTowards(firePoint.rotation, Random.rotation, Random.Range(0f, maxSpreadAngle));

        var proj = Instantiate(projectilePrefab, firePoint.position, rot);
        Destroy(proj, projectileLifeTime);

        var projScript = proj.GetComponent<Projectile>();
        projScript?.Initialize(paintLayerMask, playerColor);

        var rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = rot * Vector3.forward * projectileSpeed;

        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound);
    }
}
