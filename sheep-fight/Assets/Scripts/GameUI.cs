using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

public class GameUI : MonoBehaviour, IListener
{
    public Text wScore;
    public Text bScore;
    public Text coolDown;
    public Text ready;
    public Text account;
    public Text balance;

    public Button playButton;
    public GameController controller;
    public SheepContract contract;

    public GameObject mainMenu;

    void Start()
    {
        GameManager.GetInstance().AddListener(EVENT_TYPE.WHITE_FINISH, this);
        GameManager.GetInstance().AddListener(EVENT_TYPE.BLACK_FINISH, this);
        playButton.onClick.AddListener(OnPlay);
        UpdateScore();
    }

    void UpdateScore()
    {
        wScore.text = "" + GameManager.GetInstance().wScore;
        bScore.text = "" + GameManager.GetInstance().bScore;
    }

    public void SetAccount(string address)
    {
        account.text = address;
    }

    public void SetBalance(string balanceText)
    {
        balance.text = balanceText;
    }

    void Update()
    {
        coolDown.text = string.Format("{0:0.00}", controller.coolDown);
        ready.text = "ready: " + controller.isReady;
    }

    public async void OnPlay()
    {
        mainMenu.SetActive(false);
        string tx = await contract.Play();
        controller.Play(tx);
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
            default:
                break;
        }
    }

}
