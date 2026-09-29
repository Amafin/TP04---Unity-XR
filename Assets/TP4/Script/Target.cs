using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour
{
    [Header("Rampe")]
    [Tooltip("Rampe sur laquelle la cible se déplace (auto-détectée si vide)")]
    [SerializeField] private TargetRail rail;

    [Tooltip("Vitesse de déplacement (mètres par seconde)")]
    [SerializeField] private float moveSpeed = 1f;

    [Tooltip("Demi-largeur de la cible : elle s'arrête à cette distance des bords de la rampe")]
    [SerializeField] private float edgeMargin = 0.25f;

    [Tooltip("Choisit un sens de départ aléatoire")]
    [SerializeField] private bool randomStartDirection = true;

    [Header("Bascule en arrière")]
    [Tooltip("Tag du prefab de balle (à créer dans Tags & Layers)")]
    [SerializeField] private string bulletTag = "Bullet";

    [Tooltip("Angle de bascule en degrés. Mets une valeur négative si elle se penche vers toi au lieu de partir en arrière")]
    [SerializeField] private float tiltAngle = 90f;

    [Tooltip("Durée pour se pencher")]
    [SerializeField] private float tiltDuration = 0.25f;

    [Tooltip("Temps passé penchée avant de remonter")]
    [SerializeField] private float standUpDelay = 15f;

    [Tooltip("Durée pour remonter")]
    [SerializeField] private float riseDuration = 0.6f;

    [Tooltip("La cible pivote autour de sa base (sinon autour de son centre)")]
    [SerializeField] private bool pivotAtBottom = true;

    [Tooltip("La cible continue à glisser sur la rampe quand elle est penchée")]
    [SerializeField] private bool keepMovingWhenDown = false;

    [Tooltip("Détruire la balle à l'impact")]
    [SerializeField] private bool destroyBulletOnHit = true;

    private Rigidbody rb;

    // Position de la cible dans le repère de la rampe
    private float distanceOnRail;
    private Vector3 sideOffset;
    private Quaternion localRotation;
    private Vector3 pivotOffset;   // pivot par rapport au centre de la cible (repère de la cible, sans scale)
    private float direction = 1f;

    private float tiltAmount;      // 0 = debout, 1 = complètement penchée
    private bool isDown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (rail == null)
        {
            rail = GetComponentInParent<TargetRail>();
        }
        if (rail == null)
        {
            rail = FindFirstObjectByType<TargetRail>();
        }
        if (rail == null)
        {
            Debug.LogWarning("Aucune TargetRail trouvée pour " + name + " : la cible restera immobile.", this);
            return;
        }

        // On mémorise où la cible a été posée sur la rampe
        Vector3 local = rail.WorldToLocal(transform.position);
        distanceOnRail = ClampToRail(local.x);
        sideOffset = new Vector3(0f, local.y, local.z);
        localRotation = Quaternion.Inverse(rail.transform.rotation) * transform.rotation;

        // Pivot : le bas de la cible (ou son centre)
        if (pivotAtBottom)
        {
            Bounds b = GetComponent<Collider>().bounds;
            Vector3 bottomWorld = new Vector3(b.center.x, b.min.y, b.center.z);
            pivotOffset = Quaternion.Inverse(transform.rotation) * (bottomWorld - transform.position);
        }

        if (randomStartDirection)
        {
            direction = Random.value < 0.5f ? -1f : 1f;
        }
    }

    private void FixedUpdate()
    {
        if (rail == null) return;

        if (!isDown || keepMovingWhenDown)
        {
            MoveAlongRail();
        }

        ApplyPose();
    }

    private void MoveAlongRail()
    {
        // Va-et-vient à vitesse constante, on inverse le sens aux extrémités
        distanceOnRail += direction * moveSpeed * Time.fixedDeltaTime;

        float limit = Mathf.Max(0f, rail.HalfLength - edgeMargin);
        if (distanceOnRail > limit)
        {
            distanceOnRail = limit;
            direction = -1f;
        }
        else if (distanceOnRail < -limit)
        {
            distanceOnRail = -limit;
            direction = 1f;
        }
    }

    private void ApplyPose()
    {
        Quaternion standingRot = rail.transform.rotation * localRotation;
        Vector3 standingPos = rail.LocalToWorld(new Vector3(distanceOnRail, sideOffset.y, sideOffset.z));

        // Bascule autour de l'axe de la rampe (X), centrée sur le pivot
        Quaternion delta = Quaternion.AngleAxis(tiltAngle * tiltAmount, rail.Axis);
        Vector3 pivotWorld = standingPos + standingRot * pivotOffset;

        rb.MovePosition(pivotWorld + delta * (standingPos - pivotWorld));
        rb.MoveRotation(delta * standingRot);
    }

    private float ClampToRail(float x)
    {
        float limit = Mathf.Max(0f, rail.HalfLength - edgeMargin);
        return Mathf.Clamp(x, -limit, limit);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rail == null || isDown) return;
        if (!collision.gameObject.CompareTag(bulletTag)) return;

        if (destroyBulletOnHit)
        {
            Destroy(collision.gameObject);
        }

        StartCoroutine(TiltAndRise());
    }

    private IEnumerator TiltAndRise()
    {
        isDown = true;

        // Se penche en arrière
        yield return AnimateTilt(0f, 1f, tiltDuration);

        // Reste penchée
        yield return new WaitForSeconds(standUpDelay);

        // Remonte
        yield return AnimateTilt(1f, 0f, riseDuration);

        isDown = false;
    }

    private IEnumerator AnimateTilt(float from, float to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            tiltAmount = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        tiltAmount = to;
    }
}
