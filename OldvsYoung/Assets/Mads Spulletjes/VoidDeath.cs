using UnityEngine;

public class VoidDeath : MonoBehaviour
{
    [Header("Reset Manager")]
    public ResetManager resetManager; // Assign in Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                // Set health to 0 → triggers ResetManager automatically
                health.TakeDamage(health.currentHealth);
            }
            else
            {
                // Fallback: call ResetManager directly
                if (resetManager != null)
                    resetManager.PlayerKilled(other.gameObject);
            }
        }
    }
}
