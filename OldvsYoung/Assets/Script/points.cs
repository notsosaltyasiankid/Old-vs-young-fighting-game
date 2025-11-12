using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class points : MonoBehaviour
{
    public static points instance;

    [Header("Score UI")]
    public Text jongetje;
    public Text oldman;

    [Header("Reveal Objects (in order of score)")]
    public GameObject[] revealJongetje; 
    public GameObject[] revealOldman;   

    int scorejongetje = 0;
    int scoreoldman = 0;

    void Start()
    {
        jongetje.text = scorejongetje.ToString();
        oldman.text = scoreoldman.ToString();

        
        foreach (GameObject obj in revealJongetje)
            if (obj != null) obj.SetActive(false);

        foreach (GameObject obj in revealOldman)
            if (obj != null) obj.SetActive(false);
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
        RevealObjects(revealJongetje, scorejongetje);
    }

    public void AddPointsoldman()
    {
        scoreoldman += 1;
        oldman.text = scoreoldman.ToString();
        RevealObjects(revealOldman, scoreoldman);
    }

    void RevealObjects(GameObject[] revealList, int score)
    {
        int index = score - 1;
        if (index >= 0 && index < revealList.Length)
        {
            if (revealList[index] != null)
                revealList[index].SetActive(true);
        }
    }
}
