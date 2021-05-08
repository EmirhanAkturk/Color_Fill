using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewManager : MonoBehaviour
{
    public delegate void ViewManagerActions(bool isWin);

    [SerializeField]
    GameObject winPanel;

    [SerializeField]
    GameObject losePanel;

    private int currentSceneIndex;

    private void OnEnable()
    {
        GameManager.winLoseListener += OnWinLoseListener;
    }

    private void OnDisable()
    {
        GameManager.winLoseListener -= OnWinLoseListener;
    }

    private void Start()
    {
        losePanel?.SetActive(false);
        winPanel?.SetActive(false);

        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void OnWinLoseListener(bool isWin)
    {
        if (isWin)
            Win();
        else
            Lose();
    }

    private void Lose()
    {
        losePanel?.SetActive(true);
    }

    private void Win()
    {
        winPanel?.SetActive(true);
    }

    public void NextLevel()
    {
        int prevLevel = PlayerPrefs.GetInt("CurrentLevelNumber");
        PlayerPrefs.SetInt("CurrentLevelNumber", ++prevLevel);

        SceneManager.LoadScene((currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings); 
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(currentSceneIndex);
    }
}
