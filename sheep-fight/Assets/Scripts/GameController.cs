using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

public class GameController : MonoBehaviourPunCallbacks, IListener
{
    public Sheep[] whiteSheeps;
    public Sheep[] blackSheeps;
    public Transform[] wSpawnPositions;
    public Transform[] bSpawnPositions;

    public float coolDown = 0.0f;
    public bool isPlaying = false;
    public bool isReady = false;

    [HideInInspector]
    public List<int> sheeps;
    public SheepIcon[] icons;

    public SheepContract contract;
    public GameUI gameUI;

    private float maxCooldown;
    private Sheep currentSheep = null;
    private System.Random rand;

    string gameVersion = "1";
    bool isConnecting;
    private byte maxPlayersPerRoom = 2;

    private string playTx;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        maxCooldown = GameManager.Instance.maxCooldown;
        GameManager.Instance.AddListener(EVENT_TYPE.GAMEOVER, this);
        Connect();
    }

    public void UpdateIcons()
    {
        for (int i = 0; i < 3; i++)
        {
            icons[i].SwitchSheep(sheeps[i]);
        }
    }

    IEnumerator MockBlackSheepSpawn()
    {
        System.Random mockRand = new System.Random();
        while (true)
        {
            int sheepIdx = mockRand.Next() % 5;
            int laneIdx = mockRand.Next() % 5;
            SpawnSheeps(false, sheepIdx, laneIdx);
            icons[3].SwitchSheep(sheepIdx);
            yield return new WaitForSeconds(maxCooldown);
        }
    }

    public void SpawnBlackSheep(int sheepIdx, int laneIdx)
    {
        SpawnSheeps(false, sheepIdx, laneIdx);
        icons[3].SwitchSheep(sheepIdx);
    }

    public void NextTurn()
    {
        sheeps.RemoveAt(0);
        UpdateIcons();
        ResetCooldown();
    }

    public void Play(string tx)
    {
        isPlaying = true;
        isReady = true;
        coolDown = 0f;
        var subTx = tx.Substring(0, 8);
        int seed = Convert.ToInt32(subTx, 16);
        rand = new System.Random(seed);
        sheeps = new List<int>();
        for (int i = 0; i < 200; i++)
        {
            sheeps.Add(rand.Next() % 5);
        }
        UpdateIcons();
    }

    public void AddNewSheep()
    {
        sheeps.Add(rand.Next() % 5);
    }

    private void Update()
    {
        if (!isPlaying) return;
        if (!isReady)
        {
            coolDown -= Time.deltaTime;
            if (coolDown <= 0f)
            {
                coolDown = 0.0f;
                isReady = true;
            }
        }
    }

    public void SpawnLane(int laneIndex)
    {
        StartCoroutine(PrepareSheep(sheeps[0], laneIndex));
    }

    public IEnumerator PrepareSheep(int sheepIndex, int laneIndex)
    {
        if (currentSheep == null)
        {
            currentSheep = Instantiate<Sheep>(whiteSheeps[sheepIndex], wSpawnPositions[laneIndex].position, Quaternion.identity, wSpawnPositions[laneIndex]);
            yield return new WaitForSeconds(coolDown);
            currentSheep.BeSpawned(laneIndex);
            SendTurn(sheepIndex, laneIndex);
            NextTurn();
        }
        else
        {
            currentSheep.transform.position = wSpawnPositions[laneIndex].position;
        }
    }


    void SpawnSheeps(bool isWhite, int sheepIndex, int laneIndex)
    {
        if (isWhite)
        {
            Sheep sheep = Instantiate<Sheep>(whiteSheeps[sheepIndex], wSpawnPositions[laneIndex].position, Quaternion.identity, wSpawnPositions[laneIndex]);
            sheep.BeSpawned(laneIndex);
            SendTurn(sheepIndex, laneIndex);
        }
        else
        {
            Sheep sheep = Instantiate<Sheep>(blackSheeps[sheepIndex], bSpawnPositions[laneIndex].position, Quaternion.identity, bSpawnPositions[laneIndex]);
            sheep.BeSpawned(laneIndex);
        }

    }

    public void ResetCooldown()
    {
        coolDown = maxCooldown;
        isReady = false;
        currentSheep = null;
    }

    public void GameOver()
    {
        isPlaying = false;
        StopAllCoroutines();
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.GAMEOVER:
                GameOver();
                break;
        }
    }

    #region network implement

    public void SetNickName(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    public void CreateGame(string gameID = null)
    {
        PhotonNetwork.CreateRoom(gameID, new RoomOptions { MaxPlayers = maxPlayersPerRoom, PlayerTtl = 0, EmptyRoomTtl = 0 });
    }

    public void JoinGame()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void Connect()
    {
        isConnecting = true;
        if (PhotonNetwork.IsConnected)
        {
            return;
        }
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogFormat("Connected to master");
        Debug.LogFormat("Room count = {0}", PhotonNetwork.CountOfRooms);
    }

    public override async void OnDisconnected(DisconnectCause cause)
    {
        // Debug.LogWarningFormat("OnDisconnected() with reason {0}", cause);
        await contract.ForceEndGame();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateGame();
    }


    public override void OnPlayerEnteredRoom(Player other)
    {
        StartGame();
    }

    public override async void OnJoinedRoom()
    {
        string gameID = PhotonNetwork.CurrentRoom.Name;
        GameManager.Instance.currentGameID = gameID;
        playTx = await contract.Play(gameID);
        Debug.LogFormat("GameID: {0},  Play tx: {1}", gameID, playTx);
        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom) StartGame();
    }

    public void StartGame()
    {
        Play(playTx);
        gameUI.DisableWaiting();
        var players = PhotonNetwork.CurrentRoom.Players;
        // Debug.LogFormat("Start Game {0}, players: {1} -vs- {2}", PhotonNetwork.CurrentRoom.Name, players[1].NickName, players[2].NickName);
        if (!players[1].IsLocal) gameUI.SetAccount(players[1].NickName, false);
        else gameUI.SetAccount(players[2].NickName, false);
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects
        gameUI.GameOver(true);
        LeaveGame();
    }

    public override void OnLeftRoom()
    {
        // Debug.Log("gg, i quit");
        playTx = "";
        // endgame here
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
