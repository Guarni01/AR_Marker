using UnityEngine;

public class TowerMovement : MonoBehaviour
{
    [Header("Parti del cannone")]
    public Transform BaseTransform;   // parte che ruota su se stessa (asse Y)
    public Transform Barrel;          // canna, figlia della base (ruota su X)

    [Header("Rilevamento")]
    public float Range = 15f;         // raggio modificabile anche in gioco
    public string EnemyTag = "Enemy"; // tag dei bersagli

    [Header("Rotazione")]
    public float BaseSpeed = 90f;     // gradi/secondo della base
    public float BarrelSpeed = 60f;   // gradi/secondo della canna
    public float MinPitch = -10f;     // limite verso il basso
    public float MaxPitch = 60f;      // limite verso l'alto

    private Transform target;         // bersaglio attuale
    private float currentPitch;       // angolo attuale della canna

    public bool HasTarget => target != null;

    void Update()
    {
        FindTarget();

        if (target != null)
        {
            AimBase();
            AimBarrel();
        }
    }

    // Cerca il nemico più vicino entro il raggio
    void FindTarget()
    {
        target = null;
        float bestDist = Mathf.Infinity;

        // Prende tutti i collider dentro la sfera di raggio "range"
        Collider[] hits = Physics.OverlapSphere(transform.position, Range);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(EnemyTag)) continue; // ignora chi non è nemico

            float dist = (hit.transform.position - transform.position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                target = hit.transform;
            }
        }
    }

    // La base ruota solo sull'asse Y verso il bersaglio
    void AimBase()
    {
        Vector3 dir = target.position - BaseTransform.position;
        dir.y = 0f; // ignora l'altezza: solo rotazione orizzontale
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        BaseTransform.rotation = Quaternion.RotateTowards(
            BaseTransform.rotation, targetRot, BaseSpeed * Time.deltaTime);
    }

    // La canna si inclina su/giù (asse X locale) in base all'altezza del bersaglio
    void AimBarrel()
    {
        // Direzione verso il bersaglio nello spazio della base
        Vector3 localDir = BaseTransform.InverseTransformPoint(target.position)
                         - BaseTransform.InverseTransformPoint(Barrel.position);

        // Distanza orizzontale (X e Z) e altezza (Y)
        float flatDist = new Vector2(localDir.x, localDir.z).magnitude;

        // In Unity una rotazione X positiva abbassa la canna, quindi il segno è negativo
        float desiredPitch = -Mathf.Atan2(localDir.y, flatDist) * Mathf.Rad2Deg;
        desiredPitch = Mathf.Clamp(desiredPitch, -MaxPitch, -MinPitch);

        currentPitch = Mathf.MoveTowards(currentPitch, desiredPitch, BarrelSpeed * Time.deltaTime);
        Barrel.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    // Disegna il raggio nell'editor (sfera gialla quando selezioni il cannone)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}