// PlayerShooting.cs
// Based on your original PlayerShooting.cs :contentReference[oaicite:1]{index=1}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform  firePoint;
    public float      projectileSpeed = 20f;
    public float      fireRate        = 0.2f;
    public float      maxSpreadAngle  = 5f;
    public LayerMask  paintLayerMask;

    [Header("Ammo Settings")]
    [Tooltip("Maximum ammo capacity")]
    public int   maxAmmo       = 10;
    [Tooltip("Ammo ticks regenerated per second")]
    public float ammoRegenRate = 1f;
    [Tooltip("UI Image for ammo bar fill (0→1)")]
    public Image ammoBarFill;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioClip      shootSound;

    [Header("Turf Penalty Settings")]
    [Tooltip("How far down to check turf colour.")]
    public float turfCheckDistance = 1f;
    [Tooltip("Ammo‐regen multiplier when off own turf.")]
    public float turfRegenPenalty  = 0.5f;

    // Internal state
    private PlayerInput playerInput;
    private InputAction shootAction;
    private AudioSource audioSource;
    private bool        isFiring;
    private float       nextFireTime;
    private float       currentAmmo;

    // Cached player color for turf checks
    private Renderer playerRenderer;
    private Color    playerColor;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions.FindAction("Attack");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Cache our colour
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            playerColor = playerRenderer.material.color;

        // Input callbacks
        shootAction.performed += ctx => isFiring = true;
        shootAction.canceled  += ctx => isFiring = false;
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
        // Determine regen multiplier based on turf
        bool onOwnTurf = IsOnOwnTurf();
        float regenMul = onOwnTurf ? 1f : turfRegenPenalty;

        // Regenerate ammo
        if (currentAmmo < maxAmmo)
        {
            currentAmmo = Mathf.Min(
                maxAmmo,
                currentAmmo + ammoRegenRate * regenMul * Time.deltaTime
            );
            UpdateAmmoUI();
        }

        // Handle firing
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
                isFiring = false; // stop if empty
            }
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

        // Calculate spread
        Quaternion spreadRot = firePoint.rotation;
        if (maxSpreadAngle > 0f)
        {
            spreadRot = Quaternion.RotateTowards(
                firePoint.rotation,
                Random.rotation,
                Random.Range(0f, maxSpreadAngle)
            );
        }

        // Spawn projectile
        var projObj = Instantiate(
            projectilePrefab,
            firePoint.position,
            spreadRot
        );

        // Initialize its paint logic
        var projScript = projObj.GetComponent<Projectile>();
        if (projScript != null)
            projScript.Initialize(
                paintLayerMask,
                playerColor
            );

        // Give it velocity
        var rb = projObj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = spreadRot * Vector3.forward * projectileSpeed;

        // VFX & SFX
        if (muzzleFlash != null) muzzleFlash.Play();
        if (shootSound  != null && audioSource != null)
            audioSource.PlayOneShot(shootSound);
    }

    private bool IsOnOwnTurf()
    {
        RaycastHit hit;
        if (Physics.Raycast(
                transform.position,
                Vector3.down,
                out hit,
                turfCheckDistance,
                paintLayerMask))
        {
            var rend = hit.collider.GetComponent<Renderer>();
            return rend != null && rend.material.color == playerColor;
        }
        return false;
    }
}
