using UnityEngine;

public class TestSpawnManager : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject EnemyPrefab;     // il tuo prefab nemico
    public float SpawnInterval = 2f;   // secondi tra uno spawn e l'altro
    public Transform SpawnPoint;       // opzionale: se vuoto usa la posizione dello spawner
    public float Timer;

    void Update()
    {
        Timer += Time.deltaTime;

        if (Timer >= SpawnInterval)
        {
            Timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        if (EnemyPrefab == null) return;

        Vector3 pos = SpawnPoint != null ? SpawnPoint.position : transform.position;
        Instantiate(EnemyPrefab, pos, Quaternion.identity);
    }
}