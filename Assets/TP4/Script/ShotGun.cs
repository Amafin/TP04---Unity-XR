using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(XRGrabInteractable))]
public class ShotGun : MonoBehaviour
{
    [Header("Paramètres de tir")]
    [Tooltip("Emplacement au bout du canon où la balle apparaît")]
    public Transform barrelLocation;

    [Tooltip("Glisse ici le prefab de balle (ex: SM_Bullet_05)")]
    public GameObject bulletPrefab;

    [Tooltip("Force d'éjection de la balle")]
    [SerializeField] private float bulletSpeed = 50f;

    [Tooltip("Temps en secondes avant que la balle disparaisse")]
    [SerializeField] private float bulletLifetime = 4f;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        // Récupération de l'interactable XR
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Si aucun point de sortie n'est défini, on utilise l'arme elle-même
        if (barrelLocation == null)
        {
            barrelLocation = transform;
        }
    }

    private void OnEnable()
    {
        // Écoute l'action "Trigger" (gâchette) quand l'objet est tenu
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnActivated);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnActivated);
        }
    }

    private void OnActivated(ActivateEventArgs args)
    {
        Shoot();
    }

    public void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Aucun prefab de balle assigné sur " + gameObject.name);
            return;
        }

        // 1. Instancier la balle à la position et orientation du canon
        GameObject bullet = Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation);

        // 2. Récupérer ou ajouter un Rigidbody pour la physique
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
        }

        // 3. Propulser la balle vers l'avant (axe Z du canon)
        // Sur Unity 6, linearVelocity remplace velocity
        rb.linearVelocity = barrelLocation.forward * bulletSpeed;

        // 4. Détruire la balle après un certain délai
        Destroy(bullet, bulletLifetime);
    }
}