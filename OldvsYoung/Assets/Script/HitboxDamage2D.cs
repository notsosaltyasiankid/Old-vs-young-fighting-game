using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class HitboxDamage2D : MonoBehaviour
{
    [HideInInspector] public string ownerTag; // "Player1" or "Player2"
    [HideInInspector] public float damage = 10f;
    [HideInInspector] public Vector2 knockbackDirection = Vector2.right;
    [HideInInspector] public float knockbackForce = 5f;
    [HideInInspector] public float hitFreezeDuration = 0.05f;

    private HashSet<Collider2D> hitThisSwing = new HashSet<Collider2D>();

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void BeginAttack()
    {
        hitThisSwing.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (string.IsNullOrEmpty(ownerTag)) return;

        // ignore self
        if (other.transform.root.CompareTag(ownerTag)) return;

        // only hit a player
        bool isTargetPlayer = other.transform.root.CompareTag("Player1") || other.transform.root.CompareTag("Player2");
        if (!isTargetPlayer) return;

        if (hitThisSwing.Contains(other)) return;

        Health target = other.GetComponentInParent<Health>();
        if (target == null) return;

        hitThisSwing.Add(other);

        target.TakeDamage(damage);

        Rigidbody2D rb = other.attachedRigidbody ?? other.GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = knockbackDirection.sqrMagnitude > 0.0001f ? knockbackDirection.normalized : Vector2.right;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        if (hitFreezeDuration > 0f && gameObject.activeInHierarchy)
            StartCoroutine(HitFreeze(hitFreezeDuration));
    }

    private IEnumerator HitFreeze(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}