using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attacco")]
    public float Damage = 10f;          // danno per colpo
    public float AttackInterval = 1f;   // secondi tra un colpo e l'altro
    public string TowerTag = "Tower";   // tag delle difese da colpire
    public bool DieOnHit = false;       // true = il nemico si distrugge dopo il primo colpo

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
    }

    // Scatta ad ogni frame finché il nemico resta a contatto con la torre
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(TowerTag)) return;
        if (timer < AttackInterval) return;

        HealthTest Health = other.GetComponentInParent<HealthTest>();
        if (Health != null)
        {
            Health.TakeDamage(Damage);
            timer = 0f;

            if (DieOnHit) Destroy(gameObject);
        }
    }
}