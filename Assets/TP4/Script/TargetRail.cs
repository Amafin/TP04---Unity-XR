using UnityEngine;

/// <summary>
/// Rampe sur laquelle les cibles font des allers-retours.
/// Place cet objet où tu veux et oriente-le : les cibles suivent son axe X (rouge).
/// Astuce : mets ce script sur un objet VIDE, et le modèle 3D de la rampe en enfant
/// (ça évite les problèmes de scale déformé).
/// </summary>
public class TargetRail : MonoBehaviour
{
    [Tooltip("Longueur totale de la rampe (en mètres), centrée sur cet objet")]
    [SerializeField] private float length = 4f;

    public float HalfLength => length * 0.5f;

    // Repère de la rampe SANS le scale, pour que les distances restent en mètres
    public Vector3 Axis => transform.right;

    public Vector3 LocalToWorld(Vector3 local)
    {
        return transform.position + transform.rotation * local;
    }

    public Vector3 WorldToLocal(Vector3 world)
    {
        return Quaternion.Inverse(transform.rotation) * (world - transform.position);
    }

    private void OnDrawGizmos()
    {
        Vector3 a = LocalToWorld(Vector3.left * HalfLength);
        Vector3 b = LocalToWorld(Vector3.right * HalfLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.08f);
        Gizmos.DrawWireSphere(b, 0.08f);
    }
}
