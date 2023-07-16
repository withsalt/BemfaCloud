using BemfaCloud.Providers.Tcp.Abstractions.Connections;
using BemfaCloud.Providers.Tcp.Abstractions.Events;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace BemfaCloud.Providers.Tcp.Abstractions.Components
{
    public interface ITcpServer : INetBase<ITcpConnection>, ITcpServerEvents, IDisposable
    {
        Socket Instance { get; }

        IDictionary<string, ITcpConnection> Connections { get; }
    }
}