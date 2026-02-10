using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI Healthbar")]
    public Image foregroundBar;

    [HideInInspector] public bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (foregroundBar != null)
        {
            foregroundBar.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        ResetManager resetManager = FindObjectOfType<ResetManager>();
        if (resetManager != null)
        {
            resetManager.PlayerKilled(gameObject);
        }
    }
}
