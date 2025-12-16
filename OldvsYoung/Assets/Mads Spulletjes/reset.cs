using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class ResetManager : MonoBehaviour
{
    [Header("Players")]
    public GameObject player1;
    public GameObject player2;

    [Header("Health")]
    private Health hp1;
    private Health hp2;

    [Header("Health Bar Foregrounds")]
    public UnityEngine.UI.Image healthFillP1;
    public UnityEngine.UI.Image healthFillP2;

    [Header("Score UI (TextMeshPro)")]
    public TMP_Text scoreTextP1;
    public TMP_Text scoreTextP2;

    [Header("Round Settings")]
    public float resetDelay = 3f; // ⏱️ seconden voor auto reset

    private int scoreP1 = 0;
    private int scoreP2 = 0;

    private Vector3 startPosP1;
    private Vector3 startPosP2;

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
        // 🔴 Handmatige reset + score reset
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            scoreP1 = 0;
            scoreP2 = 0;
            UpdateScoreUI();
            StartCoroutine(RoundEndRoutine());
        }

        // 🟢 Check deaths
        if (!roundEnded)
        {
            if (hp1.currentHealth <= 0)
            {
                scoreP2++;
                UpdateScoreUI();
                StartCoroutine(RoundEndRoutine());
            }
            else if (hp2.currentHealth <= 0)
            {
                scoreP1++;
                UpdateScoreUI();
                StartCoroutine(RoundEndRoutine());
            }
        }
    }

    private IEnumerator RoundEndRoutine()
    {
        roundEnded = true;

        // Disable player input
        FighterMovementPlayer1 p1 = player1.GetComponent<FighterMovementPlayer1>();
        FighterMovementPlayer2 p2 = player2.GetComponent<FighterMovementPlayer2>();

        if (p1 != null) p1.DisableFighterInput(true);
        if (p2 != null) p2.DisableFighterInput(true);

        yield return new WaitForSeconds(resetDelay);

        ResetMatch();

        // Re-enable player input
        if (p1 != null) p1.DisableFighterInput(false);
        if (p2 != null) p2.DisableFighterInput(false);

        roundEnded = false;
    }

    public void ResetMatch()
    {
        Debug.Log("RESETTING MATCH");

        // Health reset
        hp1.currentHealth = hp1.maxHealth;
        hp2.currentHealth = hp2.maxHealth;

        // Healthbar reset
        healthFillP1.fillAmount = 1f;
        healthFillP2.fillAmount = 1f;

        // Position reset
        player1.transform.position = startPosP1;
        player2.transform.position = startPosP2;

        // Velocity reset
        player1.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player2.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // Reactivate players
        player1.SetActive(true);
        player2.SetActive(true);

        // Reset fighter internal state (important)
        FighterMovementPlayer1 p1 = player1.GetComponent<FighterMovementPlayer1>();
        FighterMovementPlayer2 p2 = player2.GetComponent<FighterMovementPlayer2>();

        if (p1 != null) p1.ResetFighterState();
        if (p2 != null) p2.ResetFighterState();

        Debug.Log("MATCH RESET COMPLETE");
    }

    private void UpdateScoreUI()
    {
        if (scoreTextP1 != null)
            scoreTextP1.text = scoreP1.ToString();

        if (scoreTextP2 != null)
            scoreTextP2.text = scoreP2.ToString();
    }
}
