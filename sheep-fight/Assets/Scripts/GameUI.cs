using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

using Photon.Pun;
using Photon.Realtime;

public class GameUI : MonoBehaviourPunCallbacks, IListener
{
    public Image whiteBar;
    public Text wScore;
    public Image blackBar;
    public Text bScore;
    public Text localPlayer;
    public Text otherPlayer;

    public Button resetButton;
    public Button quitButton;

    public GameObject gameOverPanel;
    public GameObject winText;
    public GameObject loseText;
    public GameObject exitPanel;
    public GameController controller;
    public GameObject loading;

    private float maxScore;

    void Start()
    {
        GameManager.Instance.AddListener(EVENT_TYPE.ACCOUNT_READY, this);
        GameManager.Instance.AddListener(EVENT_TYPE.WHITE_FINISH, this);
        GameManager.Instance.AddListener(EVENT_TYPE.BLACK_FINISH, this);
        GameManager.Instance.AddListener(EVENT_TYPE.GAMEOVER, this);

        quitButton.onClick.AddListener(QuitGame);
        resetButton.onClick.AddListener(ResetGame);

        maxScore = (float)GameManager.Instance.MAX_SCORE;

        Disable(gameOverPanel);
        Disable(loading);
        UpdateScore();
    }

    void UpdateScore()
    {
        wScore.text = "" + GameManager.Instance.wScore;
        whiteBar.fillAmount = GameManager.Instance.wScore / maxScore;
        bScore.text = "" + GameManager.Instance.bScore;
        blackBar.fillAmount = GameManager.Instance.bScore / maxScore;
    }

    public void Enable(GameObject obj)
    {
        obj.SetActive(true);
    }

    public void Disable(GameObject obj)
    {
        obj.SetActive(false);
    }

    public async void QuitGame()
    {
        await SheepContract.Instance.ForceEndGame();
        ResetGame();
    }

    public async void GameOver(bool isWon)
    {
        Enable(gameOverPanel);
        winText.SetActive(isWon);
        loseText.SetActive(!isWon);
        if (isWon)
        {
            await SheepContract.Instance.WinGame();
        }
        else
        {
            await SheepContract.Instance.LoseGame();
        }
    }

    public void ResetGame()
    {
        LeaveGame();
        GameManager.Instance.ResetGame();
        SceneManager.LoadSceneAsync("Lobby");
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

    #region network implement
    public override void OnDisconnected(DisconnectCause cause)
    {
        // await contract.ForceEndGame();
        SceneManager.LoadScene("Lobby");
    }

    public override async void OnJoinedRoom()
    {
        string gameID = PhotonNetwork.CurrentRoom.Name;
        playTx = await contract.Play(gameID);
        // Debug.LogFormat("GameID: {0},  Play tx: {1}", gameID, playTx);
        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom) StartGame();
    }

    public void StartGame()
    {
        Play(playTx);
        var players = PhotonNetwork.CurrentRoom.Players;
        // Debug.LogFormat("Start Game {0}, players: {1} -vs- {2}", PhotonNetwork.CurrentRoom.Name, players[1].NickName, players[2].NickName);
        if (!players[1].IsLocal) gameUI.SetAccount(players[1].NickName, false);
        else gameUI.SetAccount(players[2].NickName, false);
    }

    public void LeaveGame()
    {
        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects
        if (isPlaying) gameUI.GameOver(true);
        LeaveGame();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public void SendTurn(int sheepIndex, int laneIndex)
    {
        photonView.RPC("SendTurnRPC", RpcTarget.All, sheepIndex, laneIndex);
    }

    [PunRPC]
    void SendTurnRPC(int sheepIndex, int laneIndex, PhotonMessageInfo info)
    {
        // Debug.Log(string.Format("Info: {0} --- {1} -- {2}", sheepIndex, laneIndex, info.Sender.IsLocal));
        if (!info.Sender.IsLocal) SpawnBlackSheep(sheepIndex, laneIndex);
    }

    #endregion
}
