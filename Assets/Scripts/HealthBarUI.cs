using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Collega uno Slider alla vita di HealthTest (cercato sul padre).
/// Metti questo script sul Canvas della barra.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private HealthTest health;
    [Tooltip("Gira la barra verso la camera (utile per i mostri).")]
    [SerializeField] private bool faceCamera = true;
    [Tooltip("Nasconde la barra quando la vita arriva a 0 (togli la spunta per il castello).")]
    [SerializeField] private bool hideOnDeath = true;

    private Transform cam;
    private bool seenHealth;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>();
        if (health == null)
            health = GetComponentInParent<HealthTest>();
    }

    private void LateUpdate()
    {
        if (health == null || slider == null)
            return;

        // HealthTest imposta CurrentHealth in Start: fino ad allora mostro la barra piena.
        if (health.CurrentHealth > 0f)
            seenHealth = true;

        float value = 1f;
        if (seenHealth && health.MaxHealth > 0f)
            value = Mathf.Clamp01(health.CurrentHealth / health.MaxHealth);

        slider.value = value;

        if (seenHealth && health.CurrentHealth <= 0f && hideOnDeath)
        {
            slider.gameObject.SetActive(false);
            return;
        }

        if (faceCamera)
        {
            if (cam == null && Camera.main != null)
                cam = Camera.main.transform;
            if (cam != null)
                transform.rotation = Quaternion.LookRotation(cam.forward, cam.up);
        }
    }
}