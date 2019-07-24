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

    public Button playButton;
    public GameController controller;

    public GameObject mainMenu;

    void Start()
    {
        EventManager.GetInstance().AddListener(EVENT_TYPE.WHITE_FINISH, this);
        EventManager.GetInstance().AddListener(EVENT_TYPE.BLACK_FINISH, this);
        playButton.onClick.AddListener(OnPlay);
        UpdateScore();
    }

    void UpdateScore()
    {
        wScore.text = "" + GameManager.GetInstance().wScore;
        bScore.text = "" + GameManager.GetInstance().bScore;
    }

    void Update()
    {
        coolDown.text = string.Format("{0:0.00}", controller.coolDown);
        ready.text = "ready: " + controller.isReady;
    }

    public void OnPlay()
    {
        mainMenu.SetActive(false);
        EventManager.GetInstance().PostNotification(EVENT_TYPE.PLAY);
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
