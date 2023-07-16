namespace BemfaCloud.Providers.Tcp.Abstractions.Events
{
    public delegate void SocketEventHandler<TEventArgs>(TEventArgs e) where TEventArgs : NetEventArgs;
}