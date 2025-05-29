using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static int coins;

    [SerializeField] private GameObject mainPanel;
    [SerializeField] GameObject shopPanel;
    /*public static int levelToLoad;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject pauseBtn;
    [FormerlySerializedAs("click1")] [SerializeField] private AudioSource click;*/
    [SerializeField] UIManager uiManager;
    

    private void Start()
    {
        coins = PlayerPrefs.GetInt("coins", 0);
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        //levelToLoad = PlayerPrefs.GetInt("levelToLoad", 1);
    }
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

    public void ShopBtn()
    {
        shopPanel.SetActive(true);
    }

    public void ExitShopBtn()
    {
        shopPanel.SetActive(false);
    }
    
}