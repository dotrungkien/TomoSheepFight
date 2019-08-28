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
    public Button quitButton;

    public GameObject lobbyPanel;
    public GameObject inRoomPanel;

    public GameObject txConfirmPanel;
    public GameObject insufficientBalance;
    public GameObject firstTimePanel;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

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
        quitButton.onClick.AddListener(QuitGame);
        faucetButton.onClick.AddListener(CopyAndGoFaucet);

        Disable(insufficientBalance);
        Disable(playButton.gameObject);
        Disable(txConfirmPanel);

        SwitchPanel(lobbyPanel.name);

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
        PhotonNetwork.JoinRandomRoom();
    }

    public void QuitGame()
    {
        PhotonNetwork.LeaveRoom();
    }
    #endregion

    #region PUN Callbacks
    public override void OnConnectedToMaster()
    {
        // SwitchPanel(inRoomPanel.name);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options, null);
    }

    public override void OnJoinedRoom()
    {
        SwitchPanel(inRoomPanel.name);
    }

    public override void OnLeftRoom()
    {
        SwitchPanel(lobbyPanel.name);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PhotonNetwork.LoadLevel("Dummy");
    }

    #endregion

    #region Event Manager
    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.ACCOUNT_READY:
                SetAccount((string)param);
                PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
                PhotonNetwork.ConnectUsingSettings();
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
