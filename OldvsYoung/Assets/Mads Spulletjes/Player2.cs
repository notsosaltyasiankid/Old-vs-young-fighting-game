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

    // Input disabled for reset
    private bool inputDisabled = false;

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

    // Visibility helper
    void SetModelVisible(bool visible)
    {
        if (modelRenderers == null) return;
        foreach (var r in modelRenderers) r.enabled = visible;
    }

    void HandleMovement()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool left = Keyboard.current.leftArrowKey.isPressed;
        bool right = Keyboard.current.rightArrowKey.isPressed;

        float moveInput = 0f;
        if (left && !right) moveInput = -1f;
        else if (right && !left) moveInput = 1f;

        float targetSpeed = moveInput * moveSpeed;
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetSpeed, Time.deltaTime * 15f), rb.linearVelocity.y);

        if (Keyboard.current.upArrowKey.wasPressedThisFrame && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (Keyboard.current.downArrowKey.isPressed && !isGrounded && rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fastFallMultiplier - 1) * Time.deltaTime;
    }

    void HandleExtraJumpForce()
    {
        if (Keyboard.current.upArrowKey.isPressed && !isGrounded && rb.linearVelocity.y > 0)
            rb.linearVelocity += Vector2.up * extraJumpForce * Time.deltaTime;
    }

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

        DirectionalAttack chosenAttack = attack.rightAttack;
        if (Keyboard.current.leftArrowKey.isPressed) chosenAttack = attack.leftAttack;
        else if (Keyboard.current.rightArrowKey.isPressed) chosenAttack = attack.rightAttack;
        else if (Keyboard.current.upArrowKey.isPressed) chosenAttack = attack.upAttack;

        currentAttack = chosenAttack;

        if (chosenAttack.attackObject != null)
            chosenAttack.attackObject.SetActive(true);

        foreach (BoxCollider2D box in chosenAttack.hitboxes)
            if (box != null) box.enabled = true;

        yield return new WaitForSeconds(chosenAttack.attackDuration);

        HideAllAttackObjects();
        SetModelVisible(true);

        yield return new WaitForSeconds(chosenAttack.attackSpeed);
        isAttacking = false;
        currentAttack = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking || alreadyHit.Contains(collision) || currentAttack == null)
            return;

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
        }
    }

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

    public void ResetFighterState()
    {
        isStunned = false;
        isAttacking = false;
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
