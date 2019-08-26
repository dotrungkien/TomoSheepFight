using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

public class GameUI : MonoBehaviour, IListener
{
    public Image whiteBar;
    public Text wScore;
    public Image blackBar;
    public Text bScore;
    public Text account;
    public Text localPlayer;
    public Text otherPlayer;
    public Text balance;

    public Button playButton;
    public GameObject insufficientBalance;
    public Button resetButton;
    public Button quitButton;

    public GameObject firstTimePanel;
    public GameObject gameOverPanel;
    public GameObject winText;
    public GameObject loseText;
    public GameObject waitPanel;
    public GameObject exitPanel;

    public GameController controller;
    public SheepContract contract;

    public GameObject lobbyMenu;
    public GameObject gameMenu;

    public GameObject loading;

    void Start()
    {
        int isFirstTime = PlayerPrefs.GetInt("isFirstTime", 0);
        if (isFirstTime == 0)
        {
            firstTimePanel.SetActive(true);
            PlayerPrefs.SetInt("isFirstTime", 1);
        }
        else
        {
            firstTimePanel.SetActive(false);
        }
        GameManager.Instance.AddListener(EVENT_TYPE.ACCOUNT_READY, this);
        GameManager.Instance.AddListener(EVENT_TYPE.WHITE_FINISH, this);
        GameManager.Instance.AddListener(EVENT_TYPE.BLACK_FINISH, this);
        GameManager.Instance.AddListener(EVENT_TYPE.GAMEOVER, this);
        playButton.onClick.AddListener(PlayGame);
        quitButton.onClick.AddListener(QuitGame);
        resetButton.onClick.AddListener(ResetGame);
        gameOverPanel.SetActive(false);
        insufficientBalance.SetActive(false);
        DisablePlay();
        DisableLoading();
        UpdateScore();
    }

    void UpdateScore()
    {
        wScore.text = "" + GameManager.Instance.wScore;
        whiteBar.fillAmount = GameManager.Instance.wScore / (float)GameManager.Instance.MAX_SCORE;
        bScore.text = "" + GameManager.Instance.bScore;
        blackBar.fillAmount = GameManager.Instance.bScore / (float)GameManager.Instance.MAX_SCORE;
    }

    public void EnableLoading()
    {
        loading.gameObject.SetActive(true);
    }

    public void DisableLoading()
    {
        loading.gameObject.SetActive(false);
    }

    public void EnablePlay()
    {
        insufficientBalance.SetActive(false);
        playButton.gameObject.SetActive(true);
    }

    public void InsufficientBalance()
    {
        insufficientBalance.SetActive(true);
    }

    public void DisablePlay()
    {
        playButton.gameObject.SetActive(false);
    }

    public void EnableWaiting()
    {
        waitPanel.SetActive(true);
    }

    public void DisableWaiting()
    {
        waitPanel.SetActive(false);
    }

    public void SetAccount(string address, bool isLocal = true)
    {
        if (isLocal)
        {
            account.text = address;
            localPlayer.text = address;
        }
        else
        {
            otherPlayer.text = address;
        }
    }

    public void SetBalance(string balanceText)
    {
        balance.text = balanceText;
    }

    public void PlayGame()
    {
        EnableLoading();
        GameManager.Instance.ResetGame();
        controller.JoinGame();
    }

    public void PlayConfirmed()
    {
        gameMenu.SetActive(true);
        EnableWaiting();
        exitPanel.SetActive(false);
        lobbyMenu.SetActive(false);
    }

    public async void QuitGame()
    {
        controller.LeaveGame();
        ResetGame();
        await contract.ForceEndGame();
    }

    public async void GameOver(bool isWon)
    {
        gameOverPanel.SetActive(true);
        winText.SetActive(isWon);
        loseText.SetActive(!isWon);
        if (isWon) await contract.WinGame();
        else await contract.LoseGame();
    }

    public void ResetGame()
    {
        GameManager.Instance.ResetGame();
        gameOverPanel.SetActive(false);
        gameMenu.gameObject.SetActive(false);
        lobbyMenu.SetActive(true);
        controller.LeaveGame();
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.WHITE_FINISH:
                UpdateScore();
                break;
            case EVENT_TYPE.BLACK_FINISH:
                UpdateScore();
                break;
            case EVENT_TYPE.GAMEOVER:
                bool isWon = (bool)param;
                GameOver(isWon);
                break;
            default:
                break;
        }
    }

}
