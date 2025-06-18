using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class TurfPlayerShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform  firePoint;
    public float      projectileSpeed    = 20f;
    public float      fireRate           = 0.2f;
    public float      maxSpreadAngle     = 5f;
    public float      projectileLifeTime = 5f;
    public LayerMask  paintLayerMask;
    public float      turfCheckDistance  = 1f;
    public ParticleSystem muzzleFlash;
    public AudioClip       shootSound;

    [Header("Ammo UI")]
    public int   maxAmmo         = 10;
    public float ammoRegenRate   = 1f;
    public float turfRegenPenalty = 0.5f;
    public Image ammoBarFill;

    private PlayerInput  pi;
    private InputAction  attackAction;
    private AudioSource  audioSrc;
    private float        currentAmmo;
    private float        nextFireTime;
    private bool         isFiring;
    private Color        playerColor;

    void Awake()
    {
        pi = GetComponent<PlayerInput>();
        attackAction = pi.actions["Attack"];
        attackAction.performed += _ => isFiring = true;
        attackAction.canceled  += _ => isFiring = false;
    }

    void OnEnable()
    {
        attackAction.Enable();
    }

    void OnDisable()
    {
        attackAction.Disable();
    }

    void Start()
    {
        // init ammo and UI
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        // get this player’s color
        var dev = pi.devices[0];
        playerColor = PlayerManager.findColor(dev).color;

        // prepare audio source
        audioSrc = GetComponent<AudioSource>()
                   ?? (shootSound != null
                       ? gameObject.AddComponent<AudioSource>()
                       : null);
    }

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

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        // apply random spread
        var rot = firePoint.rotation;
        if (maxSpreadAngle > 0f)
            rot = Quaternion.RotateTowards(
                rot, Random.rotation, Random.Range(0f, maxSpreadAngle)
            );

        // spawn projectile
        var projGO = Instantiate(projectilePrefab, firePoint.position, rot);
        Destroy(projGO, projectileLifeTime);

        // initialize paint behavior
        var tp = projGO.GetComponent<TurfProjectile>();
        tp.Initialize(paintLayerMask, playerColor);

        // give it velocity
        if (projGO.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = rot * Vector3.forward * projectileSpeed;

        // play effects only if assigned
        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (shootSound != null && audioSrc != null)
            audioSrc.PlayOneShot(shootSound);
    }

    private void UpdateAmmoUI()
    {
        if (ammoBarFill != null)
            ammoBarFill.fillAmount = currentAmmo / maxAmmo;
    }
}
