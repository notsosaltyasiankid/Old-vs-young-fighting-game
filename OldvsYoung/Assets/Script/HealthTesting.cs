using UnityEngine;
using UnityEngine.InputSystem; // New Input System

// Press D to damage, H to heal (New Input System).
public class HealthTestKeysNewInput : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float damagePerPress = 10f;
    [SerializeField] private float healPerPress = 10f;
    [SerializeField] private HealthBarFillUI healthBar;

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return; // no keyboard available

        if (kb.dKey.wasPressedThisFrame)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damagePerPress);
            UpdateUI();
        }

        if (kb.hKey.wasPressedThisFrame)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + healPerPress);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);
    }
}