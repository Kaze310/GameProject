using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text timeText;
    public TMP_Text targetText;

    [Header("Result")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button restartButton;

    void Awake()
    {
        if (resultPanel) resultPanel.SetActive(false);
        if (restartButton) restartButton.onClick.AddListener(Restart);

        SetScore(0);
        SetTime("01:00");
        SetTarget(800);
    }

    public void SetScore(int v)   { if (scoreText)  scoreText.text  = $"Score: {v}"; }
    public void SetTime(string s) { if (timeText)   timeText.text   = $"Time: {s}"; }
    public void SetTarget(int v)  { if (targetText) targetText.text = $"Target: {v}"; }

    public void ShowResult(string msg)
    {
        if (resultText) resultText.text = msg;
        if (resultPanel) resultPanel.SetActive(true);
    }

    void Restart()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
