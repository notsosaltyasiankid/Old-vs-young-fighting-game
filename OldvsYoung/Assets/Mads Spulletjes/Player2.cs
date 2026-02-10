using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class FighterMovementPlayer2 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float fastFallMultiplier = 2f;
    public float extraJumpForce = 3f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Attack Settings")]
    public GameObject mainModel;

    [Header("Stun Settings")]
    public float stunDuration = 1.2f;
    private bool isStunned = false;

    [Header("Jump Settings")]
    public int maxExtraAirJumps = 1; // normal double jump
    private int currentExtraAirJumps = 0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;

    private SpriteRenderer[] modelRenderers;

    [System.Serializable]
    public class DirectionalAttack
    {
        public GameObject attackObject;
        public List<BoxCollider2D> hitboxes;
        public Vector2 knockbackDirection = Vector2.right;
        public float knockbackForce = 5f;
        public float damage = 10f;
        public float attackSpeed = 0.4f;
        public float attackDuration = 0.2f;
    }

    [System.Serializable]
    public class Attack
    {
        public string name = "New Attack";
        public DirectionalAttack rightAttack;
        public DirectionalAttack leftAttack;
        public DirectionalAttack upAttack;
    }

    public Attack attack1;
    public Attack attack2;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAttacking = false;
    private HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();
    private Collider2D[] playerColliders;
    private DirectionalAttack currentAttack;

    private bool inputDisabled = false;

    private enum AttackDirection { Right, Left, Up }
    private AttackDirection lastAttackDirection = AttackDirection.Right;

    private int jumpCount = 0;
    public int maxJumps = 2;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerColliders = GetComponentsInChildren<Collider2D>(true);

        if (mainModel != null)
            modelRenderers = mainModel.GetComponentsInChildren<SpriteRenderer>(true);

        HideAllAttackObjects();
        SetModelVisible(true);
        IgnoreInternalCollisions();
    }

    void Update()
    {
        if (isStunned || inputDisabled) return;

        HandleMovement();
        HandleAttacks();
        HandleExtraJumpForce();
    }

    // ================= VISUAL =================
    void SetModelVisible(bool visible)
    {
        if (modelRenderers == null) return;
        foreach (var r in modelRenderers)
            r.enabled = visible;
    }

    // ================= MOVEMENT =================
    void HandleMovement()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            jumpCount = 0;
            currentExtraAirJumps = 0; // reset air combo jumps when grounded
        }

        float moveInput = 0f;
        if (Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;
        if (Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;

        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, moveInput * moveSpeed, Time.deltaTime * 15f), rb.velocity.y);

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            if (isGrounded || currentExtraAirJumps < maxExtraAirJumps)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                if (!isGrounded) currentExtraAirJumps++; // count air jumps
                jumpCount++;
            }
        }

        if (Keyboard.current.downArrowKey.isPressed && !isGrounded && rb.velocity.y < 0)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fastFallMultiplier - 1) * Time.deltaTime;
    }

    void HandleExtraJumpForce()
    {
        if (Keyboard.current.upArrowKey.isPressed && !isGrounded && rb.velocity.y > 0)
            rb.velocity += Vector2.up * extraJumpForce * Time.deltaTime;
    }

    // ================= ATTACK =================
    void HandleAttacks()
    {
        if (isAttacking) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            StartCoroutine(PerformDirectionalAttack(attack1));
        if (Mouse.current.rightButton.wasPressedThisFrame)
            StartCoroutine(PerformDirectionalAttack(attack2));
    }

    IEnumerator PerformDirectionalAttack(Attack attack)
    {
        isAttacking = true;
        alreadyHit.Clear();

        HideAllAttackObjects();
        SetModelVisible(false);

        // Detect direction
        if (Keyboard.current.leftArrowKey.isPressed) lastAttackDirection = AttackDirection.Left;
        else if (Keyboard.current.rightArrowKey.isPressed) lastAttackDirection = AttackDirection.Right;
        else if (Keyboard.current.upArrowKey.isPressed) lastAttackDirection = AttackDirection.Up;

        switch (lastAttackDirection)
        {
            case AttackDirection.Left: currentAttack = attack.leftAttack; break;
            case AttackDirection.Right: currentAttack = attack.rightAttack; break;
            case AttackDirection.Up: currentAttack = attack.upAttack; break;
        }

        if (currentAttack.attackObject != null)
            currentAttack.attackObject.SetActive(true);

        foreach (BoxCollider2D box in currentAttack.hitboxes)
            if (box != null) box.enabled = true;

        yield return new WaitForSeconds(currentAttack.attackDuration);

        HideAllAttackObjects();
        SetModelVisible(true);

        yield return new WaitForSeconds(currentAttack.attackSpeed);

        isAttacking = false;
        currentAttack = null;
    }

    // ================= HIT DETECTION =================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking || alreadyHit.Contains(collision) || currentAttack == null) return;

        if (collision.CompareTag("Player1"))
        {
            alreadyHit.Add(collision);

            Health targetHealth = collision.GetComponent<Health>();
            if (targetHealth != null)
                targetHealth.TakeDamage(currentAttack.damage);

            Rigidbody2D enemyRb = collision.attachedRigidbody;
            if (enemyRb != null)
                enemyRb.AddForce(currentAttack.knockbackDirection.normalized * currentAttack.knockbackForce, ForceMode2D.Impulse);

            FighterMovementPlayer1 p1 = collision.GetComponent<FighterMovementPlayer1>();
            if (p1 != null)
                p1.ApplyStun(stunDuration);

            // HIT SOUND
            if (audioSource != null && hitSound != null)
                audioSource.PlayOneShot(hitSound);

            // AIR COMBO: extra jump if not grounded
            if (!isGrounded && currentExtraAirJumps < maxExtraAirJumps)
                currentExtraAirJumps++;
        }
    }

    // ================= STUN =================
    public void ApplyStun(float duration)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        isAttacking = false;

        HideAllAttackObjects();
        SetModelVisible(true);

        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    // ================= UTILITY =================
    public void ResetFighterState()
    {
        isStunned = false;
        isAttacking = false;
        currentExtraAirJumps = 0;
        HideAllAttackObjects();
        SetModelVisible(true);
    }

    public void DisableFighterInput(bool disable)
    {
        inputDisabled = disable;
    }

    private void HideAllAttackObjects()
    {
        HideAttackGroup(attack1);
        HideAttackGroup(attack2);
    }

    private void HideAttackGroup(Attack attack)
    {
        if (attack.leftAttack.attackObject != null) attack.leftAttack.attackObject.SetActive(false);
        if (attack.rightAttack.attackObject != null) attack.rightAttack.attackObject.SetActive(false);
        if (attack.upAttack.attackObject != null) attack.upAttack.attackObject.SetActive(false);

        foreach (var box in attack.leftAttack.hitboxes) if (box != null) box.enabled = false;
        foreach (var box in attack.rightAttack.hitboxes) if (box != null) box.enabled = false;
        foreach (var box in attack.upAttack.hitboxes) if (box != null) box.enabled = false;
    }

    private void IgnoreInternalCollisions()
    {
        List<Collider2D> allHitboxes = new List<Collider2D>();

        void Collect(Attack atk)
        {
            if (atk.rightAttack.hitboxes != null) allHitboxes.AddRange(atk.rightAttack.hitboxes);
            if (atk.leftAttack.hitboxes != null) allHitboxes.AddRange(atk.leftAttack.hitboxes);
            if (atk.upAttack.hitboxes != null) allHitboxes.AddRange(atk.upAttack.hitboxes);
        }

        Collect(attack1);
        Collect(attack2);

        foreach (var colA in allHitboxes)
        {
            if (colA == null) continue;

            foreach (var playerCol in playerColliders)
                if (playerCol != null && colA != playerCol)
                    Physics2D.IgnoreCollision(colA, playerCol, true);

            foreach (var colB in allHitboxes)
                if (colB != null && colA != colB)
                    Physics2D.IgnoreCollision(colA, colB, true);
        }
    }
}
