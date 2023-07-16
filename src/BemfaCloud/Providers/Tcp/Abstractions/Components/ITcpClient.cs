using BemfaCloud.Providers.Tcp.Abstractions.Connections;
using BemfaCloud.Providers.Tcp.Abstractions.Events;
using System;

namespace BemfaCloud.Providers.Tcp.Abstractions.Components
{
    public interface ITcpClient : INetBase<ITcpConnection>, ITcpClientEvents, IDisposable
    {
        /// <summary>
        /// socket is connected
        /// </summary>
        bool Connected { get; }

        ITcpConnection Connection { get; }
    }
}