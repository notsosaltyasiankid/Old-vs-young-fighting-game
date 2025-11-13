using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleFighter : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Set to Player1 on player 1 root, Player2 on player 2 root.")]
    public string ownerTag = "Player1"; // "Player1" or "Player2"

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Controls (Input System)")]
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;
    public Key jumpKey = Key.W;
    public Key punchKey = Key.C;

    [Header("Punch")]
    public BoxCollider2D punchHitbox;    // assign a child BoxCollider2D (IsTrigger = true)
    public float punchDuration = 0.15f;  // active frames
    public float punchCooldown = 0.25f;  // recovery
    public float damage = 10f;
    public float knockbackForce = 6f;
    public float hitFreezeDuration = 0.05f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isPunching;
    private int facing = 1; // 1 = right, -1 = left

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (punchHitbox == null)
            punchHitbox = GetComponentInChildren<BoxCollider2D>(true);

        if (punchHitbox != null)
        {
            punchHitbox.isTrigger = true;
            punchHitbox.enabled = false;

            var hb = punchHitbox.GetComponent<HitboxDamage2D>();
            if (hb == null) hb = punchHitbox.gameObject.AddComponent<HitboxDamage2D>();
            hb.ownerTag = ownerTag;
            hb.damage = damage;
            hb.knockbackForce = knockbackForce;
            hb.hitFreezeDuration = hitFreezeDuration;
        }
        else
        {
            Debug.LogWarning($"{name}: No punchHitbox assigned/found.");
        }
    }

    private void Update()
    {
        HandleMovement();
        HandlePunch();
    }

    private void HandleMovement()
    {
        // Ground check
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Move input
        float move = 0f;
        if (Keyboard.current[leftKey].isPressed) move -= 1f;
        if (Keyboard.current[rightKey].isPressed) move += 1f;

        if (Mathf.Abs(move) > 0.01f)
            facing = move > 0 ? 1 : -1;

        // Apply horizontal movement (snappy)
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Jump
        if (Keyboard.current[jumpKey].wasPressedThisFrame && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void HandlePunch()
    {
        if (isPunching || punchHitbox == null) return;

        if (Keyboard.current[punchKey].wasPressedThisFrame)
            StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        isPunching = true;

        // Configure hitbox for this swing
        var hb = punchHitbox.GetComponent<HitboxDamage2D>();
        if (hb != null)
        {
            hb.ownerTag = ownerTag;
            hb.damage = damage;
            hb.knockbackForce = knockbackForce;
            hb.hitFreezeDuration = hitFreezeDuration;
            hb.knockbackDirection = new Vector2(facing, 0f);
            hb.BeginAttack();
        }

        // Enable hitbox
        punchHitbox.enabled = true;

        // Active frames
        yield return new WaitForSeconds(punchDuration);

        // Disable hitbox
        punchHitbox.enabled = false;

        // Cooldown
        yield return new WaitForSeconds(punchCooldown);

        isPunching = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}