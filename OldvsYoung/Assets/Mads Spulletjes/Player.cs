using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class FighterMovementPlayer1 : MonoBehaviour
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
    public LayerMask enemyLayers;
    public GameObject mainModel;
    public float hitFreezeDuration = 0.05f;

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

        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;

        float moveInput = 0f;
        if (left && !right) moveInput = -1f;
        else if (right && !left) moveInput = 1f;

        float targetSpeed = moveInput * moveSpeed;
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetSpeed, Time.deltaTime * 15f), rb.linearVelocity.y);

        if (Keyboard.current.wKey.wasPressedThisFrame && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (Keyboard.current.sKey.isPressed && !isGrounded && rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fastFallMultiplier - 1) * Time.deltaTime;
    }

    void HandleExtraJumpForce()
    {
        if (Keyboard.current.wKey.isPressed && !isGrounded && rb.linearVelocity.y > 0)
            rb.linearVelocity += Vector2.up * extraJumpForce * Time.deltaTime;
    }

    void HandleAttacks()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame && !isAttacking)
            StartCoroutine(PerformDirectionalAttack(attack1));

        if (Keyboard.current.vKey.wasPressedThisFrame && !isAttacking)
            StartCoroutine(PerformDirectionalAttack(attack2));
    }

    IEnumerator PerformDirectionalAttack(Attack attack)
    {
        isAttacking = true;
        alreadyHit.Clear();
        HideAllAttackObjects();
        if (mainModel != null)
            mainModel.SetActive(false);

        DirectionalAttack chosenAttack = attack.rightAttack;
        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;
        bool up = Keyboard.current.wKey.isPressed;

        if (left) chosenAttack = attack.leftAttack;
        else if (right) chosenAttack = attack.rightAttack;
        else if (up) chosenAttack = attack.upAttack;

        currentAttack = chosenAttack;

        if (chosenAttack.attackObject != null)
            chosenAttack.attackObject.SetActive(true);

        foreach (BoxCollider2D box in chosenAttack.hitboxes)
            if (box != null) box.enabled = true;

        yield return new WaitForSeconds(chosenAttack.attackDuration);

        foreach (BoxCollider2D box in chosenAttack.hitboxes)
            if (box != null) box.enabled = false;

        HideAllAttackObjects();
        if (mainModel != null)
            mainModel.SetActive(true);

        yield return new WaitForSeconds(chosenAttack.attackSpeed);
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

        if (collision.CompareTag("Player2") && gameObject.CompareTag("Player1"))
        {
            alreadyHit.Add(collision);

            // Damage
            Health targetHealth = collision.GetComponent<Health>();
            if (targetHealth != null) targetHealth.TakeDamage(currentAttack.damage);

            // Knockback
            Rigidbody2D enemyRb = collision.attachedRigidbody;
            if (enemyRb != null)
            {
                Vector2 knockDir = currentAttack.knockbackDirection.normalized;
                enemyRb.AddForce(knockDir * currentAttack.knockbackForce, ForceMode2D.Impulse);
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
