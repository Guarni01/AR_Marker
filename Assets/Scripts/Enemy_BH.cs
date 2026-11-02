using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Movimento, bersagli, attacco e animazioni dello scheletro.
/// Richiede NavMeshAgent, Animator e HealthTest sul prefab del nemico.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthTest))]
public class Enemy : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private HealthTest health;

    [Header("Ricerca difese")]
    [Tooltip("Metti questo tag sul GameObject radice delle armi/difese.")]
    [SerializeField] private string defenseTag = "Defense";
    [Min(0.1f)] [SerializeField] private float defenseDetectionRadius = 4f;
    [Min(0.05f)] [SerializeField] private float targetRefreshInterval = 0.25f;

    [Header("Attacco")]
    [Min(0.1f)] [SerializeField] private float attackRange = 1.5f;
    [Min(0f)] [SerializeField] private float attackDamage = 10f;
    [Min(0.1f)] [SerializeField] private float attackInterval = 1.2f;
    [Tooltip("Se attivo, aggiungi un Animation Event che chiama ApplyAttackDamage nel frame del colpo.")]
    [SerializeField] private bool damageFromAnimationEvent;

    [Header("Morte")]
    [Min(0f)] [SerializeField] private float destroyAfterDeathSeconds = 2f;

    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    private const float TargetNavMeshSampleRadius = 10f;

    private Transform castle;
    private Transform currentTarget;
    private HealthTest currentTargetHealth;
    private float nextTargetRefresh;
    private float nextAttackTime;
    private bool isDead;
    private bool navMeshWarningShown;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthTest>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (health == null)
            health = GetComponent<HealthTest>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Piccole priorita' diverse evitano lo stallo di agenti identici nello stesso corridoio.
        if (agent != null)
        {
            agent.baseOffset = 0f;
            agent.avoidancePriority = Random.Range(40, 61);
        }

        // HealthTest resta responsabile della vita; Enemy ritarda solo la distruzione
        // per lasciare visibile l'animazione Death.
        if (health != null)
            health.DestroyOnDeath = false;
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<HealthTest>();

        health.OnDamaged.AddListener(PlayHitAnimation);
        health.OnDeath.AddListener(Die);
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.OnDamaged.RemoveListener(PlayHitAnimation);
        health.OnDeath.RemoveListener(Die);
    }

    private void Start()
    {
        castle = GameManager.Singleton != null ? GameManager.Singleton.Castle : null;
        SetCastleAsTarget();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (castle == null && GameManager.Singleton != null)
            castle = GameManager.Singleton.Castle;

        if (Time.time >= nextTargetRefresh)
        {
            nextTargetRefresh = Time.time + targetRefreshInterval;
            RefreshTarget();
        }

        if (currentTarget == null)
        {
            SetWalking(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            AttackTarget();
            return;
        }

        MoveToTarget();
    }

    private void RefreshTarget()
    {
        Transform closestDefense = FindClosestDefense();
        if (closestDefense != null)
        {
            currentTarget = closestDefense;
            currentTargetHealth = closestDefense.GetComponentInParent<HealthTest>();
            return;
        }

        SetCastleAsTarget();
    }

    private Transform FindClosestDefense()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, defenseDetectionRadius);
        Transform closest = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (Collider hit in hits)
        {
            Transform defense = hit.transform.root;
            if (!defense.CompareTag(defenseTag))
                continue;

            float distanceSqr = (defense.position - transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closest = defense;
            }
        }

        return closest;
    }

    private void SetCastleAsTarget()
    {
        currentTargetHealth = castle != null ? castle.GetComponentInChildren<HealthTest>() : null;
        currentTarget = currentTargetHealth != null ? currentTargetHealth.transform : castle;
    }

    private void MoveToTarget()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            if (!navMeshWarningShown)
            {
                Debug.LogError($"Enemy: '{name}' non si trova sulla NavMesh.", this);
                navMeshWarningShown = true;
            }

            SetWalking(false);
            return;
        }

        // L'ImageTarget e le difese possono non essere esattamente sulla NavMesh.
        // L'agente deve ricevere una destinazione valida, non il Transform grezzo.
        if (!NavMesh.SamplePosition(
                currentTarget.position,
                out NavMeshHit targetNavMeshHit,
                TargetNavMeshSampleRadius,
                agent.areaMask))
        {
            SetWalking(false);
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = attackRange;

        if (!agent.hasPath || (agent.destination - targetNavMeshHit.position).sqrMagnitude > 0.01f)
            agent.SetDestination(targetNavMeshHit.position);

        SetWalking(agent.pathPending || agent.remainingDistance > agent.stoppingDistance);
    }

    private void AttackTarget()
    {
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        SetWalking(false);

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackInterval;
        animator?.SetTrigger(Attack);

        if (!damageFromAnimationEvent)
            ApplyAttackDamage();
    }

    // Da richiamare con un Animation Event nell'istante dell'impatto, se richiesto.
    public void ApplyAttackDamage()
    {
        if (isDead || currentTargetHealth == null)
            return;

        currentTargetHealth.TakeDamage(attackDamage);
    }

    private void PlayHitAnimation()
    {
        if (!isDead)
            animator?.SetTrigger(Hit);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        animator?.SetBool(Walk, false);
        animator?.SetTrigger(IsDead);

        // Necessario affinche' l'animazione di morte sia visibile.
        Destroy(gameObject, destroyAfterDeathSeconds);
    }

    private void SetWalking(bool walking)
    {
        animator?.SetBool(Walk, walking);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, defenseDetectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
