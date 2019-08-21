using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class Launcher : MonoBehaviourPunCallbacks, IPunObservable
{
    string gameVersion = "1";
    [SerializeField]
    private byte maxPlayersPerRoom = 2;

    bool isConnecting;

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
        Debug.Log("OnConnectedToMaster()");
        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed. No random room available, so we create one.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.LogFormat("OnJoinedRoom() {0}.", PhotonNetwork.CurrentRoom.Name);
        // PhotonNetwork.LoadLevel("Room for 1");
    }

    public void ChangeNext()
    {
        turn = mockRand.Next().ToString();
        myTurn.text = string.Format("My Turn: {0}", turn);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(turn);

        }
        else
        {
            // Network player, receive data
            otherTurn.text = string.Format("Other Turn: {0}", (string)stream.ReceiveNext());
        }
    }
}
