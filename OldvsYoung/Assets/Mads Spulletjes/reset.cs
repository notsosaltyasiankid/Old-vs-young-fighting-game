using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Voor Image component

public class ResetManager : MonoBehaviour
{
    [Header("Players")]
    public GameObject player1;
    public GameObject player2;

    [Header("Start Positions")]
    private Vector3 startPosP1;
    private Vector3 startPosP2;

    [Header("Health Scripts")]
    private Health hp1;
    private Health hp2;

    [Header("Health Bars (GameObjects)")]
    public GameObject healthBarP1; // Healthbar GameObject met foreground Image
    public GameObject healthBarP2;

    void Start()
    {
        // Store starting positions
        startPosP1 = player1.transform.position;
        startPosP2 = player2.transform.position;

        // Health components
        hp1 = player1.GetComponent<Health>();
        hp2 = player2.GetComponent<Health>();
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetMatch();
        }
    }

    public void ResetMatch()
    {
        Debug.Log("RESETTING MATCH!");

        // Reset health
        if (hp1 != null) hp1.currentHealth = hp1.maxHealth;
        if (hp2 != null) hp2.currentHealth = hp2.maxHealth;

        // Reset healthbars foreground fill
        if (healthBarP1 != null)
        {
            Transform foreground1 = healthBarP1.transform.Find("Foreground");
            if (foreground1 != null)
            {
                Image fgImage1 = foreground1.GetComponent<Image>();
                if (fgImage1 != null) fgImage1.fillAmount = 1f;
            }
        }

        if (healthBarP2 != null)
        {
            Transform foreground2 = healthBarP2.transform.Find("Foreground");
            if (foreground2 != null)
            {
                Image fgImage2 = foreground2.GetComponent<Image>();
                if (fgImage2 != null) fgImage2.fillAmount = 1f;
            }
        }

        // Reset positions
        player1.transform.position = startPosP1;
        player2.transform.position = startPosP2;

        // Reset velocity
        Rigidbody2D rb1 = player1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = player2.GetComponent<Rigidbody2D>();

        if (rb1 != null) rb1.linearVelocity = Vector2.zero;
        if (rb2 != null) rb2.linearVelocity = Vector2.zero;

        // Reactivate if dead
        player1.SetActive(true);
        player2.SetActive(true);

        Debug.Log("MATCH RESET COMPLETE");
    }
}
