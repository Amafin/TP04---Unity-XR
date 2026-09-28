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
    [SerializeField] private float bulletSpeed = 3f;

    [Tooltip("Temps en secondes avant que la balle disparaisse")]
    [SerializeField] private float bulletLifetime = 4f;

    [Tooltip("Échelle finale de la balle (valeur absolue)")]
    [SerializeField] private Vector3 bulletScale = new Vector3(0.8f, 0.8f, 0.8f);

    [Tooltip("Décalage de rotation pour aligner le modèle 3D de la balle avec le canon")]
    [SerializeField] private Vector3 bulletRotationOffset = new Vector3(90f, 0f, 0f);

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
        Debug.Log("Gâchette pressée !");
        Shoot();
    }

    public void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Aucun prefab de balle assigné sur " + gameObject.name);
            return;
        }

        // 1. Définir le point et la rotation de sortie
        Transform spawnPoint = barrelLocation != null ? barrelLocation : transform;

        // 2. Instancier la balle orientée selon le canon (+ décalage du modèle)
        Quaternion bulletRotation = spawnPoint.rotation * Quaternion.Euler(bulletRotationOffset);
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, bulletRotation);

        // 3. Redimensionner la balle
        bullet.transform.localScale = bulletScale;

        // 4. Détruire la balle après bulletLifetime secondes
        Destroy(bullet, bulletLifetime);

        // 5. Ignorer la collision entre l'arme et la balle
        Collider gunCollider = GetComponent<Collider>();
        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (gunCollider != null && bulletCollider != null)
        {
            Physics.IgnoreCollision(gunCollider, bulletCollider);
        }

        // 6. Donner la vitesse vers l'avant du canon
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = spawnPoint.forward * bulletSpeed;
        }
    }
}