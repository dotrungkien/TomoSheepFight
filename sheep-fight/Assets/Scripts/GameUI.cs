using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour, IListener
{
    public Text wScore;
    public Text bScore;

    void Start()
    {
        EventManager.GetInstance().AddListener(EVENT_TYPE.WHITE_FINISH, this);
        EventManager.GetInstance().AddListener(EVENT_TYPE.BLACK_FINISH, this);
        UpdateScore();
    }

    void UpdateScore()
    {
        wScore.text = "" + GameManager.GetInstance().wScore;
        bScore.text = "" + GameManager.GetInstance().bScore;
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
