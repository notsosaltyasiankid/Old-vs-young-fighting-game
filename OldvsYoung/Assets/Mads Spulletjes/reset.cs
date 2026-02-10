using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class ResetManager : MonoBehaviour
{
    [Header("Players")]
    public GameObject player1;
    public GameObject player2;

    [Header("Health Bar Foregrounds")]
    public UnityEngine.UI.Image healthFillP1;
    public UnityEngine.UI.Image healthFillP2;

    [Header("Score UI (TextMeshPro)")]
    public TMP_Text scoreTextP1;
    public TMP_Text scoreTextP2;

    [Header("Round Settings")]
    public float resetDelay = 3f;

    private Health hp1;
    private Health hp2;

    private Vector3 startPosP1;
    private Vector3 startPosP2;

    private int scoreP1 = 0;
    private int scoreP2 = 0;

    private bool roundEnded = false;

    void Start()
    {
        startPosP1 = player1.transform.position;
        startPosP2 = player2.transform.position;

        hp1 = player1.GetComponent<Health>();
        hp2 = player2.GetComponent<Health>();

        UpdateScoreUI();
    }

    void Update()
    {
        // Manual reset
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            scoreP1 = 0;
            scoreP2 = 0;
            UpdateScoreUI();
            StartCoroutine(RoundEndRoutine());
        }
    }

    /// <summary>
    /// Called by Health.Die() or VoidDeath
    /// </summary>
    public void PlayerKilled(GameObject player)
    {
        if (roundEnded) return; // prevent double counting

        if (player.CompareTag("Player1"))
            scoreP2++;
        else if (player.CompareTag("Player2"))
            scoreP1++;

        UpdateScoreUI();
        StartCoroutine(RoundEndRoutine());
    }

    private IEnumerator RoundEndRoutine()
    {
        roundEnded = true;

        // Disable input
        FighterMovementPlayer1 p1 = player1.GetComponent<FighterMovementPlayer1>();
        FighterMovementPlayer2 p2 = player2.GetComponent<FighterMovementPlayer2>();

        if (p1 != null) p1.DisableFighterInput(true);
        if (p2 != null) p2.DisableFighterInput(true);

        yield return new WaitForSeconds(resetDelay);

        ResetMatch();

        // Re-enable input
        if (p1 != null) p1.DisableFighterInput(false);
        if (p2 != null) p2.DisableFighterInput(false);

        roundEnded = false;
    }

    public void ResetMatch()
    {
        Debug.Log("RESETTING MATCH");

        // Reset health
        hp1.currentHealth = hp1.maxHealth;
        hp2.currentHealth = hp2.maxHealth;

        hp1.isDead = false; // reset death flags
        hp2.isDead = false;

        // Reset health bars
        if (healthFillP1 != null) healthFillP1.fillAmount = 1f;
        if (healthFillP2 != null) healthFillP2.fillAmount = 1f;

        // Reset positions
        player1.transform.position = startPosP1;
        player2.transform.position = startPosP2;

        // Reset velocity
        player1.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player2.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // Reset fighter internal state
        FighterMovementPlayer1 p1 = player1.GetComponent<FighterMovementPlayer1>();
        FighterMovementPlayer2 p2 = player2.GetComponent<FighterMovementPlayer2>();

        if (p1 != null)
            p1.ResetFighterState();

        if (p2 != null)
            p2.ResetFighterState();

        // Ensure players are active
        player1.SetActive(true);
        player2.SetActive(true);

        Debug.Log("MATCH RESET COMPLETE");
    }


    private void UpdateScoreUI()
    {
        if (scoreTextP1 != null) scoreTextP1.text = scoreP1.ToString();
        if (scoreTextP2 != null) scoreTextP2.text = scoreP2.ToString();
    }
}
