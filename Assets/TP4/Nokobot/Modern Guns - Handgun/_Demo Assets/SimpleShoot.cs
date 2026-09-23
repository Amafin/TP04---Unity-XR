using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("Nokobot/Modern Guns/Simple Shoot XR")]
public class SimpleShootXR : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Location References")]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private Transform barrelLocation;
    [SerializeField] private Transform casingExitLocation;

    [Header("Settings")]
    [Tooltip("Specify time to destroy the casing/flash object")][SerializeField] private float destroyTimer = 2f;
    [Tooltip("Bullet Speed")][SerializeField] private float shotPower = 500f;
    [Tooltip("Casing Ejection Speed")][SerializeField] private float ejectPower = 150f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Awake()
    {
        // Récupération automatique du XRGrabInteractable sur cet objet
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Si vous utilisez une version XR Interaction Toolkit antérieure à la v3.0 :
        // Décommentez la ligne ci-dessous si le type au-dessus provoque une erreur :
        // grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            // S'abonne à la gâchette (trigger) de la manette quand l'arme est tenue
            grabInteractable.activated.AddListener(TriggerPulled);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(TriggerPulled);
        }
    }

    void Start()
    {
        if (barrelLocation == null)
            barrelLocation = transform;

        if (gunAnimator == null)
            gunAnimator = GetComponentInChildren<Animator>();
    }

    // Déclenché par la gâchette du contrôleur XR
    public void TriggerPulled(ActivateEventArgs args)
    {
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Fire");
        }
    }

    // Méthode alternative si vous préférez lier l'événement manuellement via l'Inspector
    public void FireGun()
    {
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Fire");
        }
    }

    // Appelé automatiquement par les Animation Events de l'animation de tir
    void Shoot()
    {
        if (muzzleFlashPrefab)
        {
            GameObject tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);
            Destroy(tempFlash, destroyTimer);
        }

        if (!bulletPrefab) return;

        GameObject bullet = Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(barrelLocation.forward * shotPower);
        }
    }

    // Appelé automatiquement par les Animation Events pour éjecter la douille
    void CasingRelease()
    {
        if (!casingExitLocation || !casingPrefab) return;

        GameObject tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation);
        Rigidbody rb = tempCasing.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower),
                casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f, 1f);
            rb.AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);
        }

        Destroy(tempCasing, destroyTimer);
    }
}