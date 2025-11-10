using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple test driver for HealthBar using the Unity Input System package.
/// - Press D to take damage, H to heal.
/// Save/replace your existing DamageTest.cs with this file and assign the HealthBar in the Inspector.
/// </summary>
public class DamageTest : MonoBehaviour
{
    public HealthBar healthBar;
    public float maxHealth = 100f;
    public float damagePerHit = 20f;
    public float healPerPickup = 15f;

    void Start()
    {
        if (healthBar != null)
        {
            healthBar.maxHealth = maxHealth;
            healthBar.SetHealth(maxHealth);
        }
    }

    void Update()
    {
        if (healthBar == null) return;

        var kb = Keyboard.current;
        if (kb == null) return; // no keyboard available (e.g., running in limited environment)

        if (kb.dKey.wasPressedThisFrame)
            healthBar.TakeDamage(damagePerHit);

        if (kb.hKey.wasPressedThisFrame)
            healthBar.Heal(healPerPickup);
    }
}