using UnityEngine;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    [Header("Players")]
    public Health player1Health;
    public Health player2Health;
    public SimpleFighter player1Controller;
    public SimpleFighter player2Controller;

    [Header("Spawn Points")]
    public Transform player1Spawn;
    public Transform player2Spawn;

    [Header("Round")]
    public float roundEndDelay = 1.0f;

    private bool roundInProgress = true;

    private void Awake()
    {
        // Auto-wire if missing (requires Player1/Player2 tags on root objects)
        if (player1Health == null)
            player1Health = GameObject.FindGameObjectWithTag("Player1")?.GetComponentInParent<Health>();
        if (player2Health == null)
            player2Health = GameObject.FindGameObjectWithTag("Player2")?.GetComponentInParent<Health>();
        if (player1Controller == null)
            player1Controller = GameObject.FindGameObjectWithTag("Player1")?.GetComponentInParent<SimpleFighter>();
        if (player2Controller == null)
            player2Controller = GameObject.FindGameObjectWithTag("Player2")?.GetComponentInParent<SimpleFighter>();

        if (player1Health != null) player1Health.OnDied += OnPlayerDied;
        if (player2Health != null) player2Health.OnDied += OnPlayerDied;
    }

    private void OnDestroy()
    {
        if (player1Health != null) player1Health.OnDied -= OnPlayerDied;
        if (player2Health != null) player2Health.OnDied -= OnPlayerDied;
    }

    private void OnPlayerDied(Health dead)
    {
        if (!roundInProgress) return;
        roundInProgress = false;

        // Award a point to the other player (uses your existing points script)
        if (points.instance != null)
        {
            if (dead == player1Health) points.instance.AddPointsoldman();    // Player2 scores
            else if (dead == player2Health) points.instance.AddPointsjongetje(); // Player1 scores
        }

        if (player1Controller) player1Controller.enabled = false;
        if (player2Controller) player2Controller.enabled = false;

        StartCoroutine(RoundResetCoroutine());
    }

    private IEnumerator RoundResetCoroutine()
    {
        yield return new WaitForSeconds(roundEndDelay);

        // Reset health
        if (player1Health) player1Health.ResetHealth();
        if (player2Health) player2Health.ResetHealth();

        // Reset positions
        if (player1Spawn && player1Health) player1Health.transform.position = player1Spawn.position;
        if (player2Spawn && player2Health) player2Health.transform.position = player2Spawn.position;

        if (player1Controller) player1Controller.enabled = true;
        if (player2Controller) player2Controller.enabled = true;

        roundInProgress = true;
    }
}