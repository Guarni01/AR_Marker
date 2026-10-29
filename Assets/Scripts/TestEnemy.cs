using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    [Header("Centro dell'orbita")]
    public string centerTag = "Tower";   // il cannone

    [Header("Orbita")]
    public float orbitRadius = 8f;       // distanza finale dal cannone
    public float orbitSpeed = 40f;       // gradi/secondo
    public float approachSpeed = 3f;     // velocità con cui raggiunge il raggio d'orbita

    [Header("Variazioni di altezza (rispetto al cannone)")]
    public float minHeight = 1f;
    public float maxHeight = 6f;
    public float heightSpeed = 2f;       // velocità del cambio quota
    public float changeTimeMin = 2f;     // ogni quanto cambia quota (min)
    public float changeTimeMax = 5f;     // ogni quanto cambia quota (max)

    private Transform center;
    private float angle;                 // angolo attuale sull'orbita (gradi)
    private float currentRadius;
    private float currentHeight;
    private float targetHeight;
    private float heightTimer;
    private int direction;               // 1 = antiorario, -1 = orario

    void Start()
    {
        GameObject c = GameObject.FindGameObjectWithTag(centerTag);
        if (c == null) return;
        center = c.transform;

        // Parto dalla posizione di spawn rispetto al cannone
        Vector3 offset = transform.position - center.position;
        angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
        currentRadius = new Vector2(offset.x, offset.z).magnitude;
        currentHeight = offset.y;

        direction = Random.value > 0.5f ? 1 : -1; // verso casuale
        PickNewHeight();
    }

    void Update()
    {
        if (center == null) return;

        // 1. Avanzo sull'orbita
        angle += direction * orbitSpeed * Time.deltaTime;

        // 2. Mi avvicino (o allontano) fino al raggio d'orbita
        currentRadius = Mathf.MoveTowards(currentRadius, orbitRadius, approachSpeed * Time.deltaTime);

        // 3. Ogni tanto scelgo una nuova quota
        heightTimer -= Time.deltaTime;
        if (heightTimer <= 0f) PickNewHeight();
        currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, heightSpeed * Time.deltaTime);

        // 4. Calcolo la posizione finale attorno al cannone
        float rad = angle * Mathf.Deg2Rad;
        transform.position = center.position + new Vector3(
            Mathf.Cos(rad) * currentRadius,
            currentHeight,
            Mathf.Sin(rad) * currentRadius);
    }

    void PickNewHeight()
    {
        targetHeight = Random.Range(minHeight, maxHeight);
        heightTimer = Random.Range(changeTimeMin, changeTimeMax);
    }
}