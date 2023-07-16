using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BemfaCloud.Models;

namespace BemfaCloud
{
    public interface IBemfaConnector : IDisposable
    {
        bool IsConnected { get; }

        Task<bool> ConnectAsync();

        Task<bool> DisconnectAsync();

        Task PublishAsync(string deviceTopic, string payload);

        Task UpdateAsync(string deviceTopic, string payload);

        void RegistListener(Action<MessageEventArgs> action);
    }
}
