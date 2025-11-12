using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }

    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth <= 0 ? maxHealth : CurrentHealth, 0, maxHealth);
    }

    private void Start()
    {
        // Notify UI of the initial value
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        SetHealth(CurrentHealth - amount);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        SetHealth(CurrentHealth + amount);
    }

    public void SetHealth(float newValue)
    {
        float clamped = Mathf.Clamp(newValue, 0f, maxHealth);
        if (Mathf.Approximately(clamped, CurrentHealth)) return;

        CurrentHealth = clamped;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void SetMaxHealth(float newMax, bool keepPercent = true)
    {
        newMax = Mathf.Max(1f, newMax);
        if (keepPercent)
        {
            float pct = maxHealth > 0 ? CurrentHealth / maxHealth : 1f;
            maxHealth = newMax;
            CurrentHealth = Mathf.Clamp(newMax * pct, 0f, maxHealth);
        }
        else
        {
            maxHealth = newMax;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
        }
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHealth < 1f) maxHealth = 1f;
        if (Application.isPlaying) return;
        CurrentHealth = Mathf.Clamp(maxHealth, 0f, maxHealth);
    }
#endif
}