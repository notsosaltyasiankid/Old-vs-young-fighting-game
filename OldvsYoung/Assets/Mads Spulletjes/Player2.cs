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
    public float hitFreezeDuration = 0.05f;

    [System.Serializable]
    public class DirectionalAttack
    {
        public GameObject attackObject;
        public List<BoxCollider2D> hitboxes;
    }

    [System.Serializable]
    public class Attack
    {
        public string name = "New Attack";
        public float damage = 10f;
        public float attackSpeed = 0.4f;
        public float attackDuration = 0.2f;
        public float knockbackForce = 5f;

        public DirectionalAttack rightAttack;
        public DirectionalAttack leftAttack;
        public DirectionalAttack upAttack;
    }

    public Attack attack1 = new Attack { name = "Light Attack", damage = 10f, attackSpeed = 0.4f, attackDuration = 0.2f, knockbackForce = 5f };
    public Attack attack2 = new Attack { name = "Heavy Attack", damage = 25f, attackSpeed = 0.8f, attackDuration = 0.3f, knockbackForce = 12f };

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAttacking = false;
    private HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();
    private Collider2D[] playerColliders;

    private Attack currentAttack;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerColliders = GetComponentsInChildren<Collider2D>(true);

        HideAllAttackObjects();
        if (mainModel != null)
            mainModel.SetActive(true);

        IgnoreInternalCollisions();
    }

    void Update()
    {
        HandleMovement();
        HandleAttacks();
        HandleExtraJumpForce();
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
        if (Mouse.current.leftButton.wasPressedThisFrame && !isAttacking)
            StartCoroutine(PerformDirectionalAttack(attack1));

        if (Mouse.current.rightButton.wasPressedThisFrame && !isAttacking)
            StartCoroutine(PerformDirectionalAttack(attack2));
    }

    IEnumerator PerformDirectionalAttack(Attack attack)
    {
        isAttacking = true;
        alreadyHit.Clear();
        currentAttack = attack;

        HideAllAttackObjects();
        if (mainModel != null)
            mainModel.SetActive(false);

        DirectionalAttack chosenAttack = attack.rightAttack;
        bool left = Keyboard.current.leftArrowKey.isPressed;
        bool right = Keyboard.current.rightArrowKey.isPressed;
        bool up = Keyboard.current.upArrowKey.isPressed;

        if (left) chosenAttack = attack.leftAttack;
        else if (right) chosenAttack = attack.rightAttack;
        else if (up) chosenAttack = attack.upAttack;

        if (chosenAttack.attackObject != null)
            chosenAttack.attackObject.SetActive(true);

        foreach (BoxCollider2D box in chosenAttack.hitboxes)
            if (box != null) box.enabled = true;

        yield return new WaitForSeconds(attack.attackDuration);

        foreach (BoxCollider2D box in chosenAttack.hitboxes)
            if (box != null) box.enabled = false;

        HideAllAttackObjects();
        if (mainModel != null)
            mainModel.SetActive(true);

        yield return new WaitForSeconds(attack.attackSpeed);
        isAttacking = false;
        currentAttack = null;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking || alreadyHit.Contains(collision) || currentAttack == null) return;

        if (collision.CompareTag("Player1") && gameObject.CompareTag("Player2"))
        {
            alreadyHit.Add(collision);

            Health targetHealth = collision.GetComponent<Health>();
            if (targetHealth != null) targetHealth.TakeDamage(currentAttack.damage);

            Rigidbody2D enemyRb = collision.attachedRigidbody;
            if (enemyRb != null)
            {
                // Knockback altijd omhoog
                enemyRb.AddForce(Vector2.up * currentAttack.knockbackForce, ForceMode2D.Impulse);
            }

            StartCoroutine(HitFreeze(hitFreezeDuration));
        }
    }

    private IEnumerator HitFreeze(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
