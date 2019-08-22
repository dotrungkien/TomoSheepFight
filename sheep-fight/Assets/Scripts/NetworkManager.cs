using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";
    bool isConnecting;
    private byte maxPlayersPerRoom = 2;

    public GameController controller;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        Connect();
    }

    public void SetNickName(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    public void Connect()
    {
        isConnecting = true;
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isConnecting) PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // Debug.LogWarningFormat("OnDisconnected() with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateGame("SHEEPREAL");
    }

    public void CreateGame(string gameID = null)
    {
        Debug.LogFormat("Create game: {0}", gameID);
        PhotonNetwork.CreateRoom(gameID, new RoomOptions { MaxPlayers = maxPlayersPerRoom, PlayerTtl = 0, EmptyRoomTtl = 0 });
    }

    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        StartGame();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom) StartGame();
    }

    public void StartGame()
    {
        var players = PhotonNetwork.CurrentRoom.Players;
        Debug.LogFormat("Start Game {0}, players: {1} -vs- {2}", PhotonNetwork.CurrentRoom.Name, players[1].NickName, players[2].NickName);
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects
        LeaveGame();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("gg, i quit");
    }

    public void SendTurn(int sheepIndex, int laneIndex)
    {
        photonView.RPC("SendTurnRPC", RpcTarget.All, sheepIndex, laneIndex);
    }

    [PunRPC]
    void SendTurnRPC(int sheepIndex, int laneIndex, PhotonMessageInfo info)
    {
        // Debug.Log(string.Format("Info: {0} --- {1} -- {2}", sheepIndex, laneIndex, info.Sender.IsLocal));
        if (!info.Sender.IsLocal) controller.SpawnBlackSheep(sheepIndex, laneIndex);
    }
}
