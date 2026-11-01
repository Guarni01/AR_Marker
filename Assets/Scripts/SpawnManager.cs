using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Si occupa solo di creare e tracciare i nemici della partita corrente.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("Lista di tutti i punti disponibili. Il manager ne seleziona tre casualmente.")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn della partita")]
    [Min(1)] [SerializeField] private int activeSpawnPoints = 3;
    [Tooltip("Numero minimo e massimo di nemici generati da ciascuno dei punti scelti.")]
    [SerializeField] private Vector2Int enemiesPerPointRange = new Vector2Int(1, 3);
    [Tooltip("Secondi minimi e massimi casuali tra due spawn consecutivi.")]
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(1f, 3f);

    public bool HasFinishedSpawning { get; private set; }

    public int AliveEnemyCount
    {
        get
        {
            aliveEnemies.RemoveAll(enemy => enemy == null);
            return aliveEnemies.Count;
        }
    }

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private Coroutine spawnRoutine;

    public void StartSpawning()
    {
        if (spawnRoutine != null)
            return;

        if (enemyPrefab == null || CountValidSpawnPoints() < activeSpawnPoints)
        {
            Debug.LogError("SpawnManager: assegna Enemy Prefab e almeno tanti Spawn Point quanti ne devono essere attivi.", this);
            HasFinishedSpawning = true;
            return;
        }

        aliveEnemies.RemoveAll(enemy => enemy == null);
        HasFinishedSpawning = false;
        spawnRoutine = StartCoroutine(SpawnRound());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        HasFinishedSpawning = true;
    }

    private IEnumerator SpawnRound()
    {
        List<Transform> selectedPoints = SelectSpawnPoints();
        List<Transform> spawnQueue = BuildSpawnQueue(selectedPoints);

        foreach (Transform spawnPoint in spawnQueue)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            aliveEnemies.Add(enemy);
            yield return new WaitForSeconds(RandomRange(spawnIntervalRange, 0.05f));
        }

        spawnRoutine = null;
        HasFinishedSpawning = true;
    }

    private List<Transform> SelectSpawnPoints()
    {
        List<Transform> available = new List<Transform>();
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
                available.Add(point);
        }

        int amount = Mathf.Clamp(activeSpawnPoints, 1, available.Count);
        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(i, available.Count);
            Transform temporary = available[i];
            available[i] = available[randomIndex];
            available[randomIndex] = temporary;
        }

        return available.GetRange(0, amount);
    }

    private List<Transform> BuildSpawnQueue(List<Transform> selectedPoints)
    {
        List<Transform> queue = new List<Transform>();
        int min = Mathf.Max(1, Mathf.Min(enemiesPerPointRange.x, enemiesPerPointRange.y));
        int max = Mathf.Max(min, Mathf.Max(enemiesPerPointRange.x, enemiesPerPointRange.y));

        foreach (Transform point in selectedPoints)
        {
            int enemiesAtThisPoint = Random.Range(min, max + 1);
            for (int i = 0; i < enemiesAtThisPoint; i++)
                queue.Add(point);
        }

        // Alterna casualmente gli spawn dei tre punti scelti.
        for (int i = 0; i < queue.Count; i++)
        {
            int randomIndex = Random.Range(i, queue.Count);
            Transform temporary = queue[i];
            queue[i] = queue[randomIndex];
            queue[randomIndex] = temporary;
        }

        return queue;
    }

    private int CountValidSpawnPoints()
    {
        int count = 0;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
                count++;
        }

        return count;
    }

    private static float RandomRange(Vector2 range, float minimumAllowed)
    {
        float minimum = Mathf.Max(minimumAllowed, Mathf.Min(range.x, range.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(range.x, range.y));
        return Random.Range(minimum, maximum);
    }
}
