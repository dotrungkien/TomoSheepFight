using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class NetworkManager : MonoBehaviourPunCallbacks, IPunObservable
{
    string gameVersion = "1";
    bool isConnecting;
    private byte maxPlayersPerRoom = 2;

    private string turn;
    public Text myTurn;
    public Text otherTurn;

    System.Random mockRand = new System.Random();

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        turn = mockRand.Next().ToString();
        myTurn.text = string.Format("My Turn: {0}", turn);
        Connect();
    }

    public void Connect()
    {
        isConnecting = true;
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        // Debug.Log("OnConnectedToMaster()");
        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // Debug.LogWarningFormat("OnDisconnected() with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // Debug.Log("OnJoinRandomFailed. No random room available, so we create one.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
        Debug.LogFormat("Start Game {0}", PhotonNetwork.CurrentRoom.Name);
        StartCoroutine(ChangeNext2());
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        // Debug.LogFormat("OnJoinedRoom() {0}. players = {1}", PhotonNetwork.CurrentRoom.Name, PhotonNetwork.CurrentRoom.PlayerCount);
        // // PhotonNetwork.LoadLevel("Room for 1");
        // if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        // {
        //     Debug.LogFormat("Start Game {0}", PhotonNetwork.CurrentRoom.Name);
        // }
    }

    public override void OnLeftRoom()
    {
        // SceneManager.LoadScene("PunBasics-Launcher");
        // GameManager.Instance.PostNotification(EVENT_TYPE.GAMEOVER, this, PhotonNetwork.CurrentRoom.Name);
    }

    public void ChangeNext()
    {
        turn = string.Format("{0} {1}", mockRand.Next() % 5, mockRand.Next() % 5);
        myTurn.text = string.Format("My Turn: {0}", turn);
    }

    IEnumerator ChangeNext2()
    {
        while (true)
        {
            turn = string.Format("{0} {1}", mockRand.Next() % 5, mockRand.Next() % 5);
            myTurn.text = string.Format("My Turn: {0}", turn);
            yield return new WaitForSeconds(3f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(turn);
        }
        else
        {
            otherTurn.text = string.Format("Other Turn: {0}", (string)stream.ReceiveNext());
        }
    }
}
