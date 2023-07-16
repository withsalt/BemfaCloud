using BemfaCloud.Providers.Tcp.Abstractions.Connections;
using BemfaCloud.Providers.Tcp.Abstractions.Events;

namespace BemfaCloud.Providers.Tcp.Abstractions.Events.Methods
{
    public interface ITcpServerEventHandleMethods : INetEventMethods<ITcpConnection, NetServerEventArgs>
    {
        void OnServerExceptionHandler(NetServerEventArgs args);
    }
}
