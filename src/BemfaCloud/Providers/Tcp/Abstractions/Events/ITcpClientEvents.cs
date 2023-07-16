using BemfaCloud.Providers.Tcp.Abstractions.Connections;

namespace BemfaCloud.Providers.Tcp.Abstractions.Events
{
    public interface ITcpClientEvents : IEvent<ITcpConnection, NetClientEventArgs<ITcpConnection>>
    {
    }
}