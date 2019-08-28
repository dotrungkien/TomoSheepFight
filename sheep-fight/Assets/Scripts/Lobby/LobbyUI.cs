using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

using Photon.Pun;
using Photon.Realtime;

public class LobbyUI : MonoBehaviourPunCallbacks, IListener
{
    [Header("Account details")]
    public Text address;
    public Text balance;

    [Header("UI Elements")]
    public Button playButton;
    public Button faucetButton;

    public GameObject lobbyPanel;
    public GameObject inRoomPanel;

    public GameObject txConfirmPanel;
    public GameObject insufficientBalance;
    public GameObject firstTimePanel;

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
        playButton.onClick.AddListener(PlayGame);
        faucetButton.onClick.AddListener(CopyAndGoFaucet);

        Disable(insufficientBalance);
        Disable(playButton.gameObject);
        Disable(txConfirmPanel);

        GameManager.Instance.AddListener(EVENT_TYPE.ACCOUNT_READY, this);
        GameManager.Instance.AddListener(EVENT_TYPE.BLANCE_UPDATE, this);
    }

    #region UI Callbacks
    public void SwitchPanel(string panelName)
    {
        lobbyPanel.SetActive(lobbyPanel.name.Equals(panelName));
        inRoomPanel.SetActive(inRoomPanel.name.Equals(panelName));
    }

    public void CopyAndGoFaucet()
    {
        SheepContract.Instance.CopyAndGoFaucet();
    }

    public void Enable(GameObject obj)
    {
        obj.SetActive(true);
    }

    public void Disable(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void SetAccount(string addressText)
    {
        address.text = addressText;
    }

    public void SetBalance(string balanceText)
    {
        balance.text = balanceText;
    }

    public void PlayGame()
    {
        Enable(txConfirmPanel);
        PhotonNetwork.JoinRandomRoom();
    }
    #endregion

    #region PUN Callbacks

    #endregion

    #region Event Manager
    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.ACCOUNT_READY:
                SetAccount((string)param);
                break;

            case EVENT_TYPE.BLANCE_UPDATE:
                decimal balanceVal = (decimal)param;
                SetBalance(string.Format("{0:0.00} Tomo", balanceVal));
                if (balanceVal > 1)
                {
                    Disable(insufficientBalance);
                    Enable(playButton.gameObject);
                }
                else
                {
                    Enable(insufficientBalance);
                    Disable(playButton.gameObject);
                }
                break;
            default:
                break;
        }
    }
    #endregion
}
