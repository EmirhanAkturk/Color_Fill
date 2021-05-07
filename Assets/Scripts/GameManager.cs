using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event ViewManager.ViewManagerActions winLoseListener;

    public static GameManager instance;

    private bool isPlaying;
    public bool IsPlaying { get => isPlaying; set => isPlaying = value; }

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        isPlaying = true;
    }

    public void LevelComplate()
    {
        isPlaying = false;
        winLoseListener?.Invoke(true);
    }

    public void LevelFail()
    {
        isPlaying = false;
        winLoseListener?.Invoke(false);
    }
}
