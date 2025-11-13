using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }

    public event Action<float, float> OnHealthChanged;
    public event Action<Health> OnDied;

    private bool isDead = false;

    private void Awake()
    {
        CurrentHealth = Mathf.Clamp(maxHealth, 0f, maxHealth);
        isDead = false;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || isDead) return;
        SetHealth(CurrentHealth - amount);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || isDead) return;
        SetHealth(CurrentHealth + amount);
    }

    public void SetHealth(float newValue)
    {
        float clamped = Mathf.Clamp(newValue, 0f, maxHealth);
        if (Mathf.Approximately(clamped, CurrentHealth)) return;

        CurrentHealth = clamped;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f && !isDead)
        {
            isDead = true;
            OnDied?.Invoke(this);
        }
    }

    public void ResetHealth()
    {
        isDead = false;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHealth < 1f) maxHealth = 1f;
        if (!Application.isPlaying)
        {
            CurrentHealth = Mathf.Clamp(maxHealth, 0f, maxHealth);
        }
    }
#endif
}