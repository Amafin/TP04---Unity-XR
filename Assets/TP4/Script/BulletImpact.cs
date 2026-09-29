using UnityEngine;

/// <summary>
/// À mettre sur le prefab de balle.
/// Les colliders restent toujours actifs (pour ne pas casser les IgnoreCollision du ShotGun).
/// Pendant les premiers instants, les impacts sont ignorés ; ensuite la balle est détruite à l'impact.
/// </summary>
public class BulletImpact : MonoBehaviour
{
    [Tooltip("Temps (en secondes) après le spawn pendant lequel les impacts sont ignorés")]
    [SerializeField] private float collisionDelay = 0.1f;

    private float spawnTime;

    private void Awake()
    {
        spawnTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        float age = Time.time - spawnTime;
        Debug.Log("[Bullet] touche : " + collision.gameObject.name + " (après " + age.ToString("F3") + " s)");

        if (age < collisionDelay) return;

        Destroy(gameObject);
    }
}