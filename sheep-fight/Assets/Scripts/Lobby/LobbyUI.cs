using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Nethereum.Web3;
using Nethereum.Signer;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;

using ExitGames.Client.Photon;

using Photon.Pun;
using Photon.Realtime;

public class LobbyUI : MonoBehaviourPunCallbacks, IListener
{
    private const string READY_PROP = "isPlayerReady";

    public Text gameIDText;

    [Header("Account details")]
    public Text address;
    public Text balance;

    [Header("UI Elements")]
    public Button playButton;
    public Button switchAccountButton;
    public Button quitButton;
    public Button importButton;
    public Button createAccButton;
    public Button copyAddressButton;
    public Button copyPrivButton;
    public InputField privateKeyInput;

    public GameObject cancelSwitchAccBtn;
    public GameObject lobbyPanel;
    public GameObject inRoomPanel;
    public GameObject invalidPrivateKey;

    public GameObject txConfirmPanel;
    public GameObject insufficientBalance;
    public GameObject switchAccountPanel;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        int isFirstTime = PlayerPrefs.GetInt("isFirstTime", 0);
        if (isFirstTime == 0)
        {
            cancelSwitchAccBtn.SetActive(false);
            switchAccountPanel.SetActive(true);
            PlayerPrefs.SetInt("isFirstTime", 1);
        }
        else
        {
            switchAccountPanel.SetActive(false);
        }

        invalidPrivateKey.SetActive(false);
        playButton.onClick.AddListener(PlayGame);
        quitButton.onClick.AddListener(QuitGame);
        importButton.onClick.AddListener(ImportAccount);
        createAccButton.onClick.AddListener(CreateAccount);
        switchAccountButton.onClick.AddListener(SwitchAccount);
        copyAddressButton.onClick.AddListener(SheepContract.Instance.CopyAddress);
        copyPrivButton.onClick.AddListener(SheepContract.Instance.CopyPrivateKey);

        Disable(insufficientBalance);
        Disable(playButton.gameObject);
        Disable(txConfirmPanel);

        if (lobbyPanel != null) SwitchPanel(lobbyPanel.name);

        GameManager.Instance.AddListener(EVENT_TYPE.NO_PRIVATE_KEY, this);
        GameManager.Instance.AddListener(EVENT_TYPE.ACCOUNT_READY, this);
        GameManager.Instance.AddListener(EVENT_TYPE.BLANCE_UPDATE, this);
        PhotonNetwork.ConnectUsingSettings();

        SheepContract.Instance.UpdateBalance();
        SheepContract.Instance.ForceEndGame();
    }

    private void Update()
    {
        if (GameManager.Instance.ConnectionOK) Enable(playButton.gameObject);
        else Disable(playButton.gameObject);
    }

    #region UI Callbacks

    public void CreateAccount()
    {
        invalidPrivateKey.SetActive(false);
        var ecKey = EthECKey.GenerateKey();
        var privateKey = ecKey.GetPrivateKey();
        PlayerPrefs.SetString("privateKey", privateKey);
        SheepContract.Instance.SwitchAccount();
        switchAccountPanel.SetActive(false);
    }

    public void ImportAccount()
    {
        string privateKey = privateKeyInput.text;
        try
        {
            var newAcc = new Account(privateKey);
        }
        catch
        {
            invalidPrivateKey.SetActive(true);
            Debug.LogError("Invalid private key");
            return;
        }
        invalidPrivateKey.SetActive(false);
        PlayerPrefs.SetString("privateKey", privateKey);
        SheepContract.Instance.SwitchAccount();
        switchAccountPanel.SetActive(false);
    }

    public void SwitchAccount()
    {
        switchAccountPanel.SetActive(true);
        cancelSwitchAccBtn.SetActive(true);
    }

    public void SwitchPanel(string panelName)
    {
        lobbyPanel.SetActive(lobbyPanel.name.Equals(panelName));
        inRoomPanel.SetActive(inRoomPanel.name.Equals(panelName));
    }

    public void Enable(GameObject obj)
    {
        if (obj != null) obj.SetActive(true);
    }

    public void Disable(GameObject obj)
    {
        if (obj != null) obj.SetActive(false);
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

    public async void QuitGame()
    {
        PhotonNetwork.LeaveRoom();
        Enable(txConfirmPanel);
        await SheepContract.Instance.ForceEndGame();
        Disable(txConfirmPanel);
    }
    #endregion

    #region PUN Callbacks
    public override void OnConnectedToMaster()
    {
        GameManager.Instance.photonOK = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
    }

    public override async void OnJoinedRoom()
    {
        if (inRoomPanel != null) SwitchPanel(inRoomPanel.name);
        // var subTx = tx.Substring(0, 8);
        // int seed = Convert.ToInt32(subTx, 16);
        // GameManager.Instance.currentSeed = seed;
        var props = new ExitGames.Client.Photon.Hashtable { { READY_PROP, true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public async override void OnLeftRoom()
    {
        GameManager.Instance.photonOK = false;
        if (lobbyPanel != null) SwitchPanel(lobbyPanel.name);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (lobbyPanel != null) SwitchPanel(lobbyPanel.name);
    }

    public override async void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Debug.LogFormat("Entered game {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        await CheckPlayersReady();
    }

    public override async void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        await CheckPlayersReady();
    }

    #endregion

    private async Task CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(READY_PROP, out isPlayerReady))
            {
                if (!(bool)isPlayerReady) return;
            }
            else
            {
                return;
            }
        }
        Debug.Log("Enable confirm panel");
        Enable(txConfirmPanel);
        string gameID = PhotonNetwork.CurrentRoom.Name;
        gameIDText.text = gameID;
        var tx = await SheepContract.Instance.Play(gameID);
        Debug.Log("complete");
        Disable(txConfirmPanel);
        PhotonNetwork.LoadLevel("Game");
    }

    #region Event Manager
    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.NO_PRIVATE_KEY:
                SwitchAccount();
                break;
            case EVENT_TYPE.ACCOUNT_READY:
                SetAccount((string)param);
                PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
                break;

            case EVENT_TYPE.BLANCE_UPDATE:
                decimal balanceVal = (decimal)param;
                SetAccount(PlayerPrefs.GetString("NickName"));
                SetBalance(string.Format("{0:0.00} Tomo", balanceVal));
                insufficientBalance.SetActive(balanceVal <= 1);
                break;
            default:
                break;
        }
    }
    #endregion
}
