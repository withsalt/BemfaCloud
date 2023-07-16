using BemfaCloud.Providers.Tcp.Abstractions.Connections;
using BemfaCloud.Providers.Tcp.Abstractions.Events;
using System;

namespace BemfaCloud.Providers.Tcp.Abstractions.Components
{
    public interface IUdpClient : INetBase<IUdpConnection>, IUdpClientEvents, IDisposable
    {
        IUdpConnection Connection { get; }
    }
}
