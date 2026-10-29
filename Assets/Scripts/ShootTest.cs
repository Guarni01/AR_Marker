using UnityEngine;

public class ShootTest : MonoBehaviour
{
    [Header("Riferimenti")]
    public TowerMovement Defense;                  // lo script di puntamento
    public Transform FirePoint;            // punta della canna (asse Z in avanti)
    public GameObject ProjectilePrefab;    // prefab del proiettile

    [Header("Sparo")]
    public float FireInterval = 0.5f;      // secondi tra un colpo e l'altro
    public bool OnlyWhenTarget = true;     // spara solo se c'è un nemico nel raggio

    private float Timer;

    void Update()
    {
        Timer += Time.deltaTime;

        // Se richiesto, non sparo senza bersaglio
        if (OnlyWhenTarget && (Defense == null || !Defense.HasTarget)) return;

        if (Timer >= FireInterval)
        {
            Timer = 0f;
            Shoot();
        }
    }

    void Shoot()
    {
        if (ProjectilePrefab == null || FirePoint == null) return;

        // Il proiettile nasce nel FirePoint con la stessa rotazione, quindi va dove punta la canna
        Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
    }
}