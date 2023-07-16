using BemfaCloud.Providers.Tcp.Abstractions.Connections;

namespace BemfaCloud.Providers.Tcp.Abstractions.Events
{
    public interface IUdpClientEvents : IEvent<IUdpConnection, NetClientEventArgs<IUdpConnection>>
    {
    }
}