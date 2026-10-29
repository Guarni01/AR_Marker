using UnityEngine;
using UnityEngine.Events;

public class HealthTest : MonoBehaviour
{
    [Header("Vita")]
    public float MaxHealth = 100f;     // modificabile anche in gioco
    public float CurrentHealth;        // vita attuale (visibile nell'Inspector)

    [Header("Morte")]
    public bool DestroyOnDeath = true; // distrugge l'oggetto quando la vita arriva a 0

    [Header("Eventi (opzionali)")]
    public UnityEvent OnDeath;         // es. effetti, punteggio, suoni
    public UnityEvent OnDamaged;       // es. flash rosso

    private bool isDead;

    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        CurrentHealth -= amount;
        OnDamaged?.Invoke();

        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }

    void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        if (DestroyOnDeath) Destroy(gameObject);
    }
}