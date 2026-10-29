using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 20f;          // velocità del proiettile
    public float Damage = 25f;         // danno inflitto al contatto
    public float LifeTime = 5f;        // si autodistrugge dopo N secondi
    public string TargetTag = "Enemy"; // chi può colpire (cambialo per proiettili nemici)

    void Start()
    {
        Destroy(gameObject, LifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TargetTag)) return;

        // Cerco Health sull'oggetto colpito (o sul suo padre)
        HealthTest health = other.GetComponentInParent<HealthTest>();
        if (health != null)
        {
            health.TakeDamage(Damage);
        }

        Destroy(gameObject); // il proiettile sparisce al contatto
    }
}