using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    /*public static int levelToLoad;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject pauseBtn;
    [FormerlySerializedAs("click1")] [SerializeField] private AudioSource click;
    

    private void Start()
    {
        levelToLoad = PlayerPrefs.GetInt("levelToLoad", 1);
    }*/
    [SerializeField] UIManager uiManager;
    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    public void ReplayBtn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseBtn()
    {
        uiManager.OpenPauseMenu();
    }

    public void ResumeBtn()
    {
        uiManager.ClosePauseMenu();
    }

    public void MenuBtn()
    {
        SceneManager.LoadScene(0);
    }

    public void ClearBtn()
    {
        PlayerPrefs.DeleteAll();
    }

    public void QuitBtn()
    {
        Application.Quit();
    }
}