using System;
using BemfaCloud.Providers.Tcp.Abstractions.Connections;

namespace BemfaCloud.Providers.Tcp.Abstractions.Listeners
{
    public interface ITcpListener : INetListener<ITcpConnection>, IDisposable
    {
    }
}
