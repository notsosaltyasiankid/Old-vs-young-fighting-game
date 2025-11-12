using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
public class points : MonoBehaviour
{
    public static points instance;


    public Text scoreText;

    int score = 0;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreText.text = score.ToString() + "POINTS";
    }


    private void Update()
    {
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            AddPoints();
        }

        if (score == 2)
        {
            SceneManager.LoadScene(1);
        }
    }
    public void AddPoints()
    {
        score += 1;
        scoreText.text = score.ToString() + "POINTS";
    }
}