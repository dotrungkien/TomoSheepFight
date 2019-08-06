using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BestHTTP.SocketIO;
using BestHTTP.WebSocket;
using Newtonsoft.Json.Linq;

public class Connection : MonoBehaviour
{
    [HideInInspector]

    private WebSocket socket = null;

    void Start()
    {
        SocketConnect();
    }

    void SocketConnect()
    {
        socket = new WebSocket(new Uri("ws://localhost:40510"));
        socket.OnOpen += OnOpen;
        socket.OnMessage += OnMessageReceived;
        socket.OnClosed += OnClosed;
        socket.OnError += OnError;
        socket.Open();
        Debug.Log("Opening Web Socket...\n");
    }

    void OnMessageReceived(WebSocket ws, string message)
    {
        Debug.Log(string.Format("-Message received: {0}\n", message));
    }

    void OnOpen(WebSocket ws)
    {
        Debug.Log(string.Format("-WebSocket Open!\n"));
        socket.Send("ahaaaaaaa");
    }

    void OnClosed(WebSocket ws, UInt16 code, string message)
    {
        Debug.Log(string.Format("-WebSocket closed! Code: {0} Message: {1}\n", code, message));
        socket = null;
    }

    void OnError(WebSocket ws, Exception ex)
    {
        string errorMsg = string.Empty;
#if !UNITY_WEBGL || UNITY_EDITOR
        if (ws.InternalRequest.Response != null)
        {
            errorMsg = string.Format("Status Code from Server: {0} and Message: {1}", ws.InternalRequest.Response.StatusCode, ws.InternalRequest.Response.Message);
        }
#endif
        Debug.Log(string.Format("-An error occured: {0}\n", (ex != null ? ex.Message : "Unknown Error " + errorMsg)));

        socket = null;
    }
}
