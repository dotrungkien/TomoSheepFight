#if !BESTHTTP_DISABLE_SIGNALR_CORE && !BESTHTTP_DISABLE_WEBSOCKET
using System;
using System.Collections.Generic;
using BestHTTP.WebSocket;

namespace BestHTTP.SignalRCore.Transports
{
    internal sealed class WebSocketTransport : ITransport
    {
        public TransportTypes TransportType { get { return TransportTypes.WebSocket; } }
        public TransferModes TransferMode { get { return TransferModes.Binary; } }

        public TransportStates State {
            get { return this._state; }
            private set
            {
                if (this._state != value)
                {
                    TransportStates oldState = this._state;
                    this._state = value;

                    if (this.OnStateChanged != null)
                        this.OnStateChanged(oldState, this._state);
                }
            }
        }
        private TransportStates _state;

        public string ErrorReason { get; private set; }

        public event Action<TransportStates, TransportStates> OnStateChanged;
        public event Action<string> OnTextData;
        public event Action<byte[]> OnBinaryData;

        private WebSocket.WebSocket webSocket;
        private HubConnection connection;

        private List<Messages.Message> messages = new List<Messages.Message>();

        public WebSocketTransport(HubConnection con)
        {
            this.connection = con;
            this.State = TransportStates.Initial;
        }

        public void StartConnect()
        {
            if (this.webSocket == null)
                this.webSocket = new WebSocket.WebSocket(this.connection.Uri);

#if !UNITY_WEBGL || UNITY_EDITOR
            // prepare the internal http request
            if (this.connection.AuthenticationProvider != null)
                this.connection.AuthenticationProvider.PrepareRequest(webSocket.InternalRequest);
#endif
            this.webSocket.OnOpen += OnOpen;
            this.webSocket.OnMessage += OnMessage;
            this.webSocket.OnBinary += OnBinary;
            this.webSocket.OnErrorDesc += OnError;
            this.webSocket.OnClosed += OnClosed;

            this.webSocket.Open();
            
            this.State = TransportStates.Connecting;
        }
        
        public void Send(byte[] msg)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                this.webSocket.Send(System.Text.Encoding.UTF8.GetString(msg, 0, msg.Length));
            else
                this.webSocket.Send(msg);
        }

        private void OnOpen(WebSocket.WebSocket webSocket)
        {
            // When our websocket connection is open, send the 'negotiation' message to the server.
            // It's not a real negotiation step, as we don't expect an answer to this.

            string json = this.connection.ComposeNegotiationMessage();

            byte[] buffer = JsonProtocol.WithSeparator(json);

            Send(buffer);
        }

        private Messages.Message ParseHandshakeResponse(string data)
        {
            Dictionary<string, object> response = BestHTTP.JSON.Json.Decode(data) as Dictionary<string, object>;

            if (response == null)
                return null;

            Messages.Message msg = new Messages.Message();
            msg.type = Messages.MessageTypes.Handshake;

            object error;
            if (response.TryGetValue("error", out error))
                msg.error = error.ToString();

            return msg;
        }

        private void TryHandleHandshakeResponse(string data)
        {
            var msg = ParseHandshakeResponse(data);

            if (string.IsNullOrEmpty(msg.error))
                this.State = TransportStates.Connected;
            else
            {
                this.ErrorReason = msg.error;
                this.State = TransportStates.Failed;
            }
        }

        private void OnMessage(WebSocket.WebSocket webSocket, string data)
        {
            if (this.State == TransportStates.Connecting)
            {
                TryHandleHandshakeResponse(data);
                return;
            }

            this.messages.Clear();
            try
            {
                this.connection.Protocol.ParseMessages(data, ref this.messages);

                this.connection.OnMessages(this.messages);
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("WebSocketTransport", "OnMessage(string)", ex);
            }
            finally
            {
                this.messages.Clear();
            }
        }

        private void OnBinary(WebSocket.WebSocket webSocket, byte[] data)
        {
            if (this.State == TransportStates.Connecting)
            {
                TryHandleHandshakeResponse(System.Text.Encoding.UTF8.GetString(data, 0, data.Length));

                return;
            }

            this.messages.Clear();
            try
            {
                this.connection.Protocol.ParseMessages(data, ref this.messages);

                this.connection.OnMessages(this.messages);
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("WebSocketTransport", "OnMessage(byte[])", ex);
            }
            finally
            {
                this.messages.Clear();
            }
        }

        private void OnError(WebSocket.WebSocket webSocket, string reason)
        {
            this.ErrorReason = reason;
            this.State = TransportStates.Failed;
        }

        private void OnClosed(WebSocket.WebSocket webSocket, ushort code, string message)
        {
            this.webSocket = null;

            this.State = TransportStates.Closed;
        }

        public void StartClose()
        {
            if (this.webSocket != null)
                this.webSocket.Close();
            this.State = TransportStates.Closing;
        }
    }
}
#endif