#if !BESTHTTP_DISABLE_SIGNALR_CORE && !BESTHTTP_DISABLE_WEBSOCKET
using System;

namespace BestHTTP.SignalRCore.Messages
{
    public enum MessageTypes : int
    {
        Handshake  = 0,
        Invocation = 1,
        StreamItem = 2,
        Completion = 3,
        StreamInvocation = 4,
        CancelInvocation = 5,
        Ping = 6
    }

    public class Message
    {
        public MessageTypes type;
        public string invocationId;
        public bool nonblocking;
        public string target;
        public object[] arguments;
        public object item;
        public object result;
        public string error;

        public override string ToString()
        {
            switch (this.type)
            {
                case MessageTypes.Invocation:
                    return string.Format("[Invocation Id: {0}, Target: '{1}', Argument count: {2}]", this.invocationId, this.target, this.arguments != null ? this.arguments.Length : 0);
                case MessageTypes.StreamItem:
                    return string.Format("[StreamItem Id: {0}, Item: {1}]", this.invocationId, this.item.ToString());
                case MessageTypes.Completion:
                    return string.Format("[Completion Id: {0}, Result: {1}, Error: '{2}']", this.invocationId, this.result, this.error);
                case MessageTypes.StreamInvocation:
                    return string.Format("[StreamInvocation Id: {0}, Target: '{1}', Argument count: {2}]", this.invocationId, this.target, this.arguments != null ? this.arguments.Length : 0);
                case MessageTypes.CancelInvocation:
                    return string.Format("[CancelInvocation Id: {0}]", this.invocationId);
                case MessageTypes.Ping:
                    return "[Ping]";
                default:
                    throw new Exception("Unknown message! Type: " + this.type);
            }
        }
    }
}
#endif