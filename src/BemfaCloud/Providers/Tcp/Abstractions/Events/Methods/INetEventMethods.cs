using BemfaCloud.Providers.Tcp.Abstractions.Connections;
using BemfaCloud.Providers.Tcp.Abstractions.Events;

namespace BemfaCloud.Providers.Tcp.Abstractions.Events.Methods
{
    public interface INetEventMethods<TConnection, TEventArgs>
        where TConnection : INetConnection
        where TEventArgs : NetEventArgs
    {
        void OnConnectedHandler(NetClientEventArgs<TConnection> args);

        void OnReceivedHandler(NetClientReceivedEventArgs<TConnection> args);

        void OnDisconnectedHandler(NetClientEventArgs<TConnection> args);

        void OnStartedHandler(TEventArgs args);

        void OnStoppedHandler(TEventArgs args);

        void OnExceptionHandler(NetClientEventArgs<TConnection> args);
    }
}