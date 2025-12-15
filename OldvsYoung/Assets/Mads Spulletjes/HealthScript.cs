using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI Healthbar")]
    public Image foregroundBar;   // De gevulde balk (foreground)

    void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage! Current Health: {currentHealth}");

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
            float fill = currentHealth / maxHealth;
            foregroundBar.fillAmount = fill;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        gameObject.SetActive(false);
    }
}
