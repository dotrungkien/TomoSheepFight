#if !BESTHTTP_DISABLE_SIGNALR_CORE && !BESTHTTP_DISABLE_WEBSOCKET
using BestHTTP.Futures;
using BestHTTP.SignalR.Authentication;
using System;
using System.Collections.Generic;

namespace BestHTTP.SignalRCore
{
    public sealed class HubConnection
    {
        public static readonly object[] EmptyArgs = new object[0];

        /// <summary>
        /// Uri  of the Hub endpoint
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Current state of this connection.
        /// </summary>
        public ConnectionStates State { get; private set; }

        /// <summary>
        /// The preferred transport to choose.
        /// </summary>
        public TransportTypes PreferedTransport { get; private set; }

        /// <summary>
        /// Current, active ITransport instance.
        /// </summary>
        public ITransport Transport { get; private set; }

        /// <summary>
        /// The IProtocol implementation that will parse, encode and decode messages.
        /// </summary>
        public IProtocol Protocol { get; private set; }

        /// <summary>
        /// This event is called when successfully connected to the hub.
        /// </summary>
        public event Action<HubConnection> OnConnected;

        /// <summary>
        /// This event is called when an unexpected error happen and the connection is closed.
        /// </summary>
        public event Action<HubConnection, string> OnError;

        /// <summary>
        /// This event is called when the connection is gracefully terminated.
        /// </summary>
        public event Action<HubConnection> OnClosed;

        /// <summary>
        /// This event is called for every server-sent message. When returns false, no further processing of the message is done
        /// by the plugin.
        /// </summary>
        public event Func<HubConnection, Messages.Message, bool> OnMessage;

        /// <summary>
        /// An IAuthenticationProvider implementation that will be used to authenticate the connection.
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// This will be increment to add a unique id to every message the plugin will send.
        /// </summary>
        private long lastInvocationId = 0;
        
        private Dictionary<long, Action<Messages.Message>> invocations = new Dictionary<long, Action<Messages.Message>>();
        private Dictionary<string, Subscription> subscriptions = new Dictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);

        public HubConnection(Uri hubUri, IProtocol protocol)
            : this(hubUri, protocol, TransportTypes.WebSocket)
        {
        }

        public HubConnection(Uri hubUri, IProtocol protocol, TransportTypes preferedTransport)
        {
            this.Uri = hubUri;
            this.State = ConnectionStates.Initial;
            this.PreferedTransport = preferedTransport;
            this.Protocol = protocol;
            this.Protocol.Connection = this;
        }

        public void StartConnect()
        {
            if (this.State != ConnectionStates.Initial)
                return;

            HTTPManager.Logger.Verbose("HubConnection", "StartConnect");

            if (this.AuthenticationProvider != null && this.AuthenticationProvider.IsPreAuthRequired)
            {
                this.State = ConnectionStates.Authenticating;

                this.AuthenticationProvider.OnAuthenticationSucceded += OnAuthenticationSucceded;
                this.AuthenticationProvider.OnAuthenticationFailed += OnAuthenticationFailed;

                // Start the authentication process
                this.AuthenticationProvider.StartAuthentication();
            }
            else
                ConnectImpl();
        }

        private void OnAuthenticationSucceded(SignalR.Authentication.IAuthenticationProvider provider)
        {
            HTTPManager.Logger.Verbose("HubConnection", "OnAuthenticationSucceded");

            this.AuthenticationProvider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
            this.AuthenticationProvider.OnAuthenticationFailed -= OnAuthenticationFailed;

            ConnectImpl();
        }

        private void OnAuthenticationFailed(SignalR.Authentication.IAuthenticationProvider provider, string reason)
        {
            HTTPManager.Logger.Error("HubConnection", "OnAuthenticationFailed: " + reason);

            this.AuthenticationProvider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
            this.AuthenticationProvider.OnAuthenticationFailed -= OnAuthenticationFailed;

            this.State = ConnectionStates.Closed;
            if (this.OnError != null)
                this.OnError(this, reason);
        }
        
        private void ConnectImpl()
        {
            HTTPManager.Logger.Verbose("HubConnection", "ConnectImpl");

            switch (this.PreferedTransport)
            {
                case TransportTypes.WebSocket:
                    this.Transport = new Transports.WebSocketTransport(this);
                    this.Transport.OnStateChanged += Transport_OnStateChanged;
                    break;
            }

            this.State = ConnectionStates.Negotiating;
            this.Transport.StartConnect();
        }

        public void StartClose()
        {
            HTTPManager.Logger.Verbose("HubConnection", "StartClose");

            if (this.Transport != null)
                this.Transport.StartClose();
        }

        public IFuture<StreamItemContainer<TResult>> Stream<TResult>(string target, params object[] args)
        {
            var future = new Future<StreamItemContainer<TResult>>();
            
            long id = InvokeImp(target,
                        args,
                        callback: (message) =>
                        {
                            switch (message.type)
                            {
                                // StreamItem message contains only one item.
                                case Messages.MessageTypes.StreamItem:
                                    {
                                        var container = future.value;

                                        if (container.IsCanceled)
                                            break;

                                        container.AddItem((TResult)this.Protocol.ConvertTo(typeof(TResult), message.item));

                                        // (re)assign the container to raise OnItem event
                                        future.AssignItem(container);
                                        break;
                                    }

                                case Messages.MessageTypes.Completion:
                                    {
                                        bool isSuccess = string.IsNullOrEmpty(message.error);
                                        if (isSuccess)
                                        {
                                            var container = future.value;

                                            // While completion message must not contain any result, this should be future-proof
                                            //if (!container.IsCanceled && message.Result != null)
                                            //{
                                            //    TResult[] results = (TResult[])this.Protocol.ConvertTo(typeof(TResult[]), message.Result);
                                            //
                                            //    container.AddItems(results);
                                            //}

                                            future.Assign(container);
                                        }
                                        else
                                            future.Fail(new Exception(message.error));
                                        break;
                                    }
                            }
                        }, 
                        isStreamingInvocation: true);

            future.BeginProcess(new StreamItemContainer<TResult>(id));

            return future;
        }

        public void CancelStream<T>(StreamItemContainer<T> container)
        {
            Messages.Message message = new Messages.Message {
                type = Messages.MessageTypes.CancelInvocation,
                invocationId = container.id.ToString()
            };

            container.IsCanceled = true;

            SendMessage(message);
        }

        public IFuture<TResult> Invoke<TResult>(string target, params object[] args)
        {
            Future<TResult> future = new Future<TResult>();

            InvokeImp(target,
                args,
                (message) =>
                    {
                        bool isSuccess = string.IsNullOrEmpty(message.error);
                        if (isSuccess)
                            future.Assign((TResult)this.Protocol.ConvertTo(typeof(TResult), message.result));
                        else
                            future.Fail(new Exception(message.error));
                    });

            return future;
        }

        public IFuture<bool> Send(string target, params object[] args)
        {
            Future<bool> future = new Future<bool>();

            InvokeImp(target,
                args,
                (message) =>
                    {
                        bool isSuccess = string.IsNullOrEmpty(message.error);
                        if (isSuccess)
                            future.Assign(true);
                        else
                            future.Fail(new Exception(message.error));
                    });

            return future;
        }

        private long InvokeImp(string target, object[] args, Action<Messages.Message> callback, bool isStreamingInvocation = false)
        {
            if (this.State != ConnectionStates.Connected)
                throw new Exception("Not connected yet!");

            long invocationId = System.Threading.Interlocked.Increment(ref this.lastInvocationId);
            var message = new Messages.Message
            {
                type = isStreamingInvocation ? Messages.MessageTypes.StreamInvocation : Messages.MessageTypes.Invocation,
                invocationId = invocationId.ToString(),
                target = target,
                arguments = args,
                nonblocking = callback == null,
            };

            SendMessage(message);

            if (callback != null)
                this.invocations.Add(invocationId, callback);

            return invocationId;
        }

        private void SendMessage(Messages.Message message)
        {
            byte[] encoded = this.Protocol.EncodeMessage(message);
            this.Transport.Send(encoded);
        }

        public void On(string methodName, Action callback)
        {
            On(methodName, null, (args) => callback());
        }

        public void On<T1>(string methodName, Action<T1> callback)
        {
            On(methodName, new Type[] { typeof(T1) }, (args) => callback((T1)args[0]));
        }

        public void On<T1, T2>(string methodName, Action<T1, T2> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2) },
                (args) => callback((T1)args[0], (T2)args[1]));
        }

        public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2), typeof(T3) },
                (args) => callback((T1)args[0], (T2)args[1], (T3)args[2]));
        }

        public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) },
                (args) => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]));
        }

        public void On(string methodName, Type[] paramTypes, Action<object[]> callback)
        {
            Subscription subscription = null;
            if (!this.subscriptions.TryGetValue(methodName, out subscription))
                this.subscriptions.Add(methodName, subscription = new Subscription());

            subscription.Add(paramTypes, callback);
        }
        
        private void MessageArrived(Messages.Message message)
        {
            try
            {
                if (this.OnMessage != null && !this.OnMessage(this, message))
                    return;
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("HubConnection", "MessageArrived - OnMessage", ex);
            }

            try
            {
                switch (message.type)
                {
                    case Messages.MessageTypes.Invocation:
                        {
                            Subscription subscribtion = null;
                            if (this.subscriptions.TryGetValue(message.target, out subscribtion))
                            {
                                for (int i = 0; i < subscribtion.callbacks.Count; ++i)
                                {
                                    var callbackDesc = subscribtion.callbacks[i];

                                    var realArgs = this.Protocol.GetRealArguments(callbackDesc.ParamTypes, message.arguments);

                                    callbackDesc.Callback.Invoke(realArgs);
                                }
                            }

                            break;
                        }

                    case Messages.MessageTypes.StreamItem:
                        {
                            long invocationId;
                            if (long.TryParse(message.invocationId, out invocationId))
                            {
                                Action<Messages.Message> callback;
                                if (this.invocations.TryGetValue(invocationId, out callback) && callback != null)
                                    callback(message);
                            }
                            break;
                        }

                    case Messages.MessageTypes.Completion:
                        {
                            long invocationId;
                            if (long.TryParse(message.invocationId, out invocationId))
                            {
                                Action<Messages.Message> callback;
                                if (this.invocations.TryGetValue(invocationId, out callback) && callback != null)
                                    callback(message);
                                this.invocations.Remove(invocationId);
                            }
                            break;
                        }
                }
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("HubConnection", "MessageArrived", ex);
            }
        }

        internal string ComposeNegotiationMessage()
        {
            return string.Format("{{'protocol':'{0}', 'version': 1}}", this.Protocol.Encoder.Name);
        }

        internal void OnMessages(List<Messages.Message> messages)
        {
            for (int i = 0; i < messages.Count; ++i)
                this.MessageArrived(messages[i]);
        }

        private void Transport_OnStateChanged(TransportStates oldState, TransportStates newState)
        {
            HTTPManager.Logger.Verbose("HubConnection", string.Format("Transport_OnStateChanged - oldState: {0} newState: {1}", oldState.ToString(), newState.ToString()));

            switch (newState)
            {
                case TransportStates.Connected:
                    this.State = ConnectionStates.Connected;
                    if (this.OnConnected != null)
                        this.OnConnected(this);
                    break;

                case TransportStates.Failed:
                    this.State = ConnectionStates.Closed;
                    if (this.OnError != null)
                        this.OnError(this, this.Transport.ErrorReason);
                    break;

                case TransportStates.Closed:
                    this.State = ConnectionStates.Closed;
                    if (this.OnClosed != null)
                        this.OnClosed(this);
                    break;
            }
        }
    }
}

#endif