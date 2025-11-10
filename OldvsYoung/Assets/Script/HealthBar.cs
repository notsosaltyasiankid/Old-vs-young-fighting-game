using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple health bar with a "lagging damage" effect:
/// - foregroundImage (green) updates instantly to the new health
/// - backgroundImage (red) lags and smoothly interpolates down to the foreground, revealing red under the green
/// Works with Image.type = Filled (Fill Method = Horizontal, Origin = Left) or with full-width Images using fillAmount.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Images (assign in inspector)")]
    public Image foregroundImage; // green, on top
    public Image backgroundImage; // red, behind

    [Header("Health")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Animation")]
    [Tooltip("How long the red (background) takes to catch up to the green (foreground) after taking damage (seconds).")]
    public float backgroundCatchUpDuration = 0.6f;
    [Tooltip("Optional small delay before the red starts falling to the new value (seconds).")]
    public float backgroundDelay = 0.05f;
    [Tooltip("Use SmoothDamp-like smoothing instead of linear Lerp when background animates.")]
    public bool useSmooth = false;

    private Coroutine backgroundCoroutine;

    private void Start()
    {
        // clamp and initialize visuals
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        float fill = HealthToFill(currentHealth);
        if (foregroundImage) foregroundImage.fillAmount = fill;
        if (backgroundImage) backgroundImage.fillAmount = fill;
    }

    // Utility: convert health to Image.fillAmount in [0,1]
    private float HealthToFill(float health)
    {
        if (maxHealth <= 0f) return 0f;
        return Mathf.Clamp01(health / maxHealth);
    }

    /// <summary>
    /// Set current health (0..maxHealth). Updates the bar with the lagging red effect if health decreased.
    /// Call this to apply damage or healing.
    /// </summary>
    public void SetHealth(float newHealth)
    {
        newHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        float oldFill = HealthToFill(currentHealth);
        float newFill = HealthToFill(newHealth);

        currentHealth = newHealth;

        // Foreground updates instantly to show current health (green shrinks/grows immediately)
        if (foregroundImage)
            foregroundImage.fillAmount = newFill;

        // If background exists, animate it only when background is higher than newFill (damage),
        // or when healing we can immediately set background to newFill (or animate differently if desired).
        if (backgroundImage)
        {
            // If there is an existing animation, stop it
            if (backgroundCoroutine != null)
                StopCoroutine(backgroundCoroutine);

            // If damage (background was showing more than new)
            if (oldFill > newFill)
            {
                backgroundCoroutine = StartCoroutine(AnimateBackground(oldFill, newFill));
            }
            else
            {
                // For healing: move background to newFill immediately (so no red shows when you heal),
                // or you could animate it to the new value if you want a different effect.
                backgroundImage.fillAmount = newFill;
            }
        }
    }

    private IEnumerator AnimateBackground(float from, float to)
    {
        if (backgroundDelay > 0f)
            yield return new WaitForSeconds(backgroundDelay);

        float elapsed = 0f;
        while (elapsed < backgroundCatchUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / backgroundCatchUpDuration);
            if (useSmooth)
                t = Mathf.SmoothStep(0f, 1f, t);

            float val = Mathf.Lerp(from, to, t);
            backgroundImage.fillAmount = val;
            yield return null;
        }

        backgroundImage.fillAmount = to;
        backgroundCoroutine = null;
    }

    // Optional helper methods for convenience
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        SetHealth(currentHealth - amount);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        SetHealth(currentHealth + amount);
    }

    // Expose current health if needed
    public float GetCurrentHealth() => currentHealth;
}