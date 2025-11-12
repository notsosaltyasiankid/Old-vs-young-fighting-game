using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class FighterMovementWithBoxHitboxesAndAttackObjects : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float fastFallMultiplier = 2f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Attack Settings")]
    public LayerMask enemyLayers;

    [Header("Player Model")]
    [Tooltip("Assign the main player GameObject or model that should hide during attacks.")]
    public GameObject mainModel;

    [System.Serializable]
    public class Attack
    {
        public string name = "New Attack";
        public Key key;
        public List<BoxCollider2D> hitboxes;
        public GameObject attackObject;
        public float damage = 10f;
        public float attackSpeed = 0.4f;
        public float hitFreeze = 0.1f;
        public float attackDuration = 0.2f;
    }

    public enum Key { C, V }

    public Attack attack1 = new Attack { name = "Light Attack", key = Key.C, damage = 10, attackSpeed = 0.4f, hitFreeze = 0.1f, attackDuration = 0.2f };
    public Attack attack2 = new Attack { name = "Heavy Attack", key = Key.V, damage = 25, attackSpeed = 0.8f, hitFreeze = 0.15f, attackDuration = 0.3f };

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAttacking = false;
    private bool isFrozen = false;
    private HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        DisableHitboxesAndAttackObjects();

        // Make sure hitboxes ignore each other
        foreach (BoxCollider2D box in attack1.hitboxes)
            if (box != null) Physics2D.IgnoreLayerCollision(box.gameObject.layer, box.gameObject.layer, true);
        foreach (BoxCollider2D box in attack2.hitboxes)
            if (box != null) Physics2D.IgnoreLayerCollision(box.gameObject.layer, box.gameObject.layer, true);
    }

    void Update()
    {
        if (!isFrozen) // Only allow movement if the game is not frozen
            HandleMovement();

        HandleAttacks(); // Can trigger attacks even if moving
    }

    void HandleMovement()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;

        float moveInput = 0f;
        if (left && !right) moveInput = -1f;
        else if (right && !left) moveInput = 1f;

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (Keyboard.current.wKey.wasPressedThisFrame && isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        if (Keyboard.current.sKey.isPressed && !isGrounded && rb.velocity.y < 0)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fastFallMultiplier - 1) * Time.deltaTime;
    }

    void HandleAttacks()
    {
        if (isAttacking) return;

        if (Keyboard.current.cKey.wasPressedThisFrame)
            StartCoroutine(PerformAttack(attack1));

        if (Keyboard.current.vKey.wasPressedThisFrame)
            StartCoroutine(PerformAttack(attack2));
    }

    IEnumerator PerformAttack(Attack attack)
    {
        isAttacking = true;
        alreadyHit.Clear();

        // Hide main model
        if (mainModel != null) mainModel.SetActive(false);

        // Show attack object
        if (attack.attackObject != null) attack.attackObject.SetActive(true);

        // Enable hitboxes
        foreach (BoxCollider2D box in attack.hitboxes)
            if (box != null) box.enabled = true;

        float timer = 0f;
        while (timer < attack.attackDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Disable hitboxes
        foreach (BoxCollider2D box in attack.hitboxes)
            if (box != null) box.enabled = false;

        // Hide attack object and restore main model
        if (attack.attackObject != null) attack.attackObject.SetActive(false);
        if (mainModel != null) mainModel.SetActive(true);

        yield return new WaitForSeconds(attack.attackSpeed);
        isAttacking = false;
    }

    IEnumerator GlobalHitFreeze(float duration)
    {
        isFrozen = true;
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
        isFrozen = false;
    }

    void DisableHitboxesAndAttackObjects()
    {
        foreach (BoxCollider2D box in attack1.hitboxes)
            if (box != null) box.enabled = false;
        if (attack1.attackObject != null)
            attack1.attackObject.SetActive(false);

        foreach (BoxCollider2D box in attack2.hitboxes)
            if (box != null) box.enabled = false;
        if (attack2.attackObject != null)
            attack2.attackObject.SetActive(false);

        if (mainModel != null)
            mainModel.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking || alreadyHit.Contains(collision)) return;

        if (((1 << collision.gameObject.layer) & enemyLayers) != 0)
        {
            Debug.Log($"Hit {collision.name} for damage!");
            alreadyHit.Add(collision);

            // Freeze the whole game for hit duration
            StartCoroutine(GlobalHitFreeze(0.1f));
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        foreach (BoxCollider2D box in attack1.hitboxes)
            if (box != null) Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
        foreach (BoxCollider2D box in attack2.hitboxes)
            if (box != null) Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
    }
}
