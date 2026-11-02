using UnityEngine;
using UnityEngine.Events;
using Unity.AI.Navigation;

public class GameManager : MonoBehaviour
{
    // Accesso globale: GameManager.Singleton.StartGame();
    public static GameManager Singleton { get; private set; }

    public enum GameState
    {
        WaitingForCastle,
        Playing,
        Victory,
        Defeat
    }

    [Header("Riferimenti")]
    [Tooltip("Trasform del castello. Deve contenere HealthTest, anche in un figlio.")]
    [SerializeField] private Transform castle;
    [SerializeField] private SpawnManager spawnManager;
    private NavMeshSurface navMeshSurface;

    [Header("Eventi")]
    [SerializeField] private UnityEvent onVictory;
    [SerializeField] private UnityEvent onDefeat;

    [Header("Test PC")]
    [Tooltip("Solo per test nell'Editor: avvia la partita senza attendere Vuforia.")]
    [SerializeField] private bool startAutomaticallyForPcTest;

    public GameState CurrentState { get; private set; } = GameState.WaitingForCastle;
    public Transform Castle => castle;
    private HealthTest castleHealth;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        CacheCastleHealth();
        CacheNavMeshSurface();
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    private void Start()
    {
        if (startAutomaticallyForPcTest)
            StartGame();
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        if (castle == null || castleHealth == null || castleHealth.CurrentHealth <= 0f)
        {
            EndGame(false);
            return;
        }

        // La vittoria arriva solo dopo che tutti i nemici previsti sono comparsi e morti.
        if (spawnManager.HasFinishedSpawning && spawnManager.AliveEnemyCount == 0)
            EndGame(true);
    }

    // Collegare questo metodo al callback "Target Found" di Vuforia.
    public void StartGame()
    {
        if (CurrentState != GameState.WaitingForCastle)
            return;

        CacheCastleHealth();
        CacheNavMeshSurface();

        if (castle == null || castleHealth == null || spawnManager == null || navMeshSurface == null)
        {
            Debug.LogError("GameManager: assegna Castle (con HealthTest), Spawn Manager e NavMesh Surface.", this);
            return;
        }

        // In build le mesh sorgenti non sono necessariamente leggibili a runtime.
        // Usiamo la NavMesh gia' baked nel prefab e la registriamo nella posa
        // corrente del target Vuforia, senza tentare di ricostruirla.
        navMeshSurface.RemoveData();
        navMeshSurface.AddData();

        CurrentState = GameState.Playing;
        spawnManager.StartSpawning();
    }

    // Utile se lo script Vuforia deve passare direttamente il castello riconosciuto.
    public void StartGame(Transform castleTransform)
    {
        castle = castleTransform;
        StartGame();
    }

    private void EndGame(bool victory)
    {
        if (CurrentState != GameState.Playing)
            return;

        spawnManager.StopSpawning();

        CurrentState = victory ? GameState.Victory : GameState.Defeat;

        if (victory)
            onVictory?.Invoke();
        else
            onDefeat?.Invoke();
    }

    private void CacheCastleHealth()
    {
        castleHealth = castle != null ? castle.GetComponentInChildren<HealthTest>() : null;
    }

    private void CacheNavMeshSurface()
    {
        if (navMeshSurface == null && castle != null)
            navMeshSurface = castle.GetComponentInChildren<NavMeshSurface>(true);
    }

}
