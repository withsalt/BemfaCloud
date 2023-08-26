using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Models;

namespace BemfaCloud.Connectors.Base
{
    public abstract class BaseConnector : IBemfaConnector
    {
        protected BaseConnectorBuilder Builder { get; private set; }

        protected abstract event Action<ErrorEventArgs> OnError;
        protected abstract event Action<MessageEventArgs> OnMessage;

        public abstract bool IsConnected { get; }

        public abstract ProtocolType ProtocolType { get; }

        public BaseConnector(BaseConnectorBuilder builder)
        {
            Builder = builder;
        }

        public abstract void Dispose();

        public abstract Task<bool> ConnectAsync();

        public abstract Task<bool> DisconnectAsync();

        public abstract Task PublishAsync(string deviceTopic, string payload);

        public abstract Task UpdateAsync(string deviceTopic, string payload);

        public virtual void MessageEventRegister(Action<MessageEventArgs> action)
        {
            if (action != null) 
                this.OnMessage += action;
        }
    }
}
