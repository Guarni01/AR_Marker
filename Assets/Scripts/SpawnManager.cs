using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Si occupa solo di creare e tracciare i nemici della partita corrente.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Configurazione")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [Min(1)] [SerializeField] private int activeSpawnPoints = 3;
    [SerializeField] private Vector2Int enemiesPerPointRange = new Vector2Int(1, 3);
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(1f, 3f);

    // In AR il callback di Vuforia e la registrazione della NavMesh possono
    // avvenire nello stesso frame. Un margine leggermente piu' ampio evita che
    // uno Spawn Point vicino al bordo venga scartato inutilmente.
    private const float NavMeshSampleRadius = 5f;
    private const float SpawnScatterRadius = 1.5f;
    private const float MinimumEnemySeparation = 1.1f;
    private const int SpawnPositionAttempts = 8;

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
        // Lascia terminare il frame in cui GameManager registra la NavMesh
        // nella posa corrente dell'ImageTarget prima di cercare le posizioni.
        yield return null;

        List<Transform> selectedPoints = SelectSpawnPoints();
        List<Transform> spawnQueue = BuildSpawnQueue(selectedPoints);

        foreach (Transform spawnPoint in spawnQueue)
        {
            if (!TryFindFreeNavMeshPosition(spawnPoint, out Vector3 spawnPosition))
            {
                Debug.LogError($"SpawnManager: nessuna posizione NavMesh libera vicino al punto '{spawnPoint.name}'.", spawnPoint);
                continue;
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, spawnPoint.rotation);
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

            if (agent == null || !agent.Warp(spawnPosition))
            {
                Debug.LogError($"SpawnManager: impossibile posizionare '{enemy.name}' sulla NavMesh.", enemy);
                Destroy(enemy);
                continue;
            }

            aliveEnemies.Add(enemy);
            yield return new WaitForSeconds(RandomRange(spawnIntervalRange, 0.05f));
        }

        spawnRoutine = null;
        HasFinishedSpawning = true;
    }

    private bool TryFindFreeNavMeshPosition(Transform spawnPoint, out Vector3 position)
    {
        for (int attempt = 0; attempt < SpawnPositionAttempts; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * SpawnScatterRadius;
            Vector3 candidate = spawnPoint.position + new Vector3(offset.x, 0f, offset.y);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
                continue;

            if (IsFarEnoughFromAliveEnemies(hit.position))
            {
                position = hit.position;
                return true;
            }
        }

        position = default;
        return false;
    }

    private bool IsFarEnoughFromAliveEnemies(Vector3 candidate)
    {
        aliveEnemies.RemoveAll(enemy => enemy == null);
        float minimumDistanceSqr = MinimumEnemySeparation * MinimumEnemySeparation;

        foreach (GameObject enemy in aliveEnemies)
        {
            Vector3 difference = enemy.transform.position - candidate;
            difference.y = 0f;

            if (difference.sqrMagnitude < minimumDistanceSqr)
                return false;
        }

        return true;
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
