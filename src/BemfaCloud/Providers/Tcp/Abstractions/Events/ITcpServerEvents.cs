using BemfaCloud.Providers.Tcp.Abstractions.Connections;

namespace BemfaCloud.Providers.Tcp.Abstractions.Events
{
    public interface ITcpServerEvents : IEvent<ITcpConnection, NetServerEventArgs>
    {
        SocketEventHandler<NetServerEventArgs> OnServerException { get; set; }
    }
}