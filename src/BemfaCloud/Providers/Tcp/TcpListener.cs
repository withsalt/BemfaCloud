using BemfaCloud.Providers.Tcp.Abstractions.Connections;
using BemfaCloud.Providers.Tcp.Abstractions.Events;
using BemfaCloud.Providers.Tcp.Abstractions.Listeners;
using System;
using System.Net;
using System.Net.Sockets;

namespace BemfaCloud.Providers.Tcp
{
    internal sealed class TcpListener : NetListener<ITcpConnection>, INetListener<ITcpConnection>, ITcpListener, IDisposable
    {
        public TcpListener(ITcpConnection connection) : base(connection)
        {
        }

        protected override void OnBeginReceive(ref byte[] buffers) => Connection.Instance.BeginReceive(buffers, 0, buffers.Length, SocketFlags.None, ReceivedCallback, null);

        protected override T OnEndReceive<T>(IAsyncResult result) => new ReceiveResult(Connection.Instance.EndReceive(result, out SocketError error), Connection.RemoteEndPoint) as T;

        protected override void OnReceiveHandling(byte[] buffers, IPEndPoint remote = null, IPEndPoint local = null) => Connection.OnReceived?.Invoke(new NetClientReceivedEventArgs<ITcpConnection>(Connection, buffers, remote, local));

        protected override void OnStop(Exception exception) => Connection?.OnDisconnected?.Invoke(new NetClientEventArgs<ITcpConnection>(Connection) { Exception = exception });

        protected override void OnException(Exception exception) => Connection?.OnException?.Invoke(new NetClientEventArgs<ITcpConnection>(Connection) { Exception = exception });

        public override void Dispose()
        {
            if (Running && Connection?.Instance.Connected == true)
            {
                try
                {
                    Connection?.Instance?.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException e)
                {
                    OnException(e);
                }

                try
                {
                    Connection?.Instance?.Close();
                }
                catch (SocketException e)
                {
                    OnException(e);
                }
            }
            try
            {
                Connection?.Instance?.Dispose();
            }
            catch (Exception e)
            {
                OnException(e);
            }
            Connection = null;
        }
    }
}
