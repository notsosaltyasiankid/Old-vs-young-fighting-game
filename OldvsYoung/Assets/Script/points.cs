using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
public class points : MonoBehaviour
{
    public static points instance;


    public Text jongetje;
    public Text oldman;

    int scorejongetje = 0;
    int scoreoldman = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jongetje.text = scorejongetje.ToString();
        oldman.text = scoreoldman.ToString();
    }


    private void Update()
    {
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            AddPointsjongetje();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            AddPointsoldman();
        }

        if (scorejongetje == 3)
        {
            SceneManager.LoadScene(1);
        }

        if (scoreoldman == 3)
        {
            SceneManager.LoadScene(2);
        }
    }
    public void AddPointsjongetje()
    {
        scorejongetje += 1;
        jongetje.text = scorejongetje.ToString();
    }

    public void AddPointsoldman()
    {
        scoreoldman += 1;
        oldman.text = scoreoldman.ToString();
    }
}