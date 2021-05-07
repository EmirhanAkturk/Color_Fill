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
        losePanel.SetActive(false);
        winPanel.SetActive(false);
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
        losePanel.SetActive(true);
    }

    private void Win()
    {
        winPanel.SetActive(true);
    }

    public void NextLevel()
    {

    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
