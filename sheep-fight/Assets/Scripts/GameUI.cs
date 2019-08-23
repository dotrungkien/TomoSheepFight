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
    public Text balance;


    public Button playButton;
    public Button resetButton;
    public Button quitButton;

    public GameObject gameOverPanel;
    public GameObject winText;
    public GameObject loseText;
    public GameObject waitPanel;

    public GameController controller;
    public SheepContract contract;

    public GameObject lobbyMenu;
    public GameObject gameMenu;

    void Start()
    {
        GameManager.Instance.AddListener(EVENT_TYPE.ACCOUNT_READY, this);
        GameManager.Instance.AddListener(EVENT_TYPE.WHITE_FINISH, this);
        GameManager.Instance.AddListener(EVENT_TYPE.BLACK_FINISH, this);
        GameManager.Instance.AddListener(EVENT_TYPE.GAMEOVER, this);
        playButton.onClick.AddListener(PlayGame);
        quitButton.onClick.AddListener(QuitGame);
        resetButton.onClick.AddListener(ResetGame);
        gameOverPanel.SetActive(false);
        UpdateScore();
    }

    void UpdateScore()
    {
        wScore.text = "" + GameManager.Instance.wScore;
        whiteBar.fillAmount = GameManager.Instance.wScore / 100f;
        bScore.text = "" + GameManager.Instance.bScore;
        blackBar.fillAmount = GameManager.Instance.bScore / 100f;
    }

    public void EnableWaiting()
    {
        waitPanel.SetActive(true);
    }

    public void DisableWaiting()
    {
        waitPanel.SetActive(false);
    }

    public void SetAccount(string address)
    {
        account.text = address;
    }

    public void SetBalance(string balanceText)
    {
        balance.text = balanceText;
    }

    public void PlayGame()
    {
        gameMenu.SetActive(true);
        EnableWaiting();
        lobbyMenu.SetActive(false);
        controller.JoinGame();
    }

    public async void QuitGame()
    {
        lobbyMenu.gameObject.SetActive(true);
        gameMenu.gameObject.SetActive(false);
        await contract.ForceEndGame();
        await contract.SetBalance();
    }

    public async void GameOver(bool isWon)
    {
        gameOverPanel.SetActive(true);
        winText.SetActive(isWon);
        loseText.SetActive(!isWon);
        // await contract.EndGame(isWon);
    }

    public void ResetGame()
    {
        gameOverPanel.SetActive(false);
        lobbyMenu.SetActive(true);
        GameManager.Instance.ResetGame();
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.ACCOUNT_READY:
                playButton.gameObject.SetActive(true);
                break;
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
