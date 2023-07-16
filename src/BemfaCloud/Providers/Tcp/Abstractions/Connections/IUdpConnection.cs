using BemfaCloud.Providers.Tcp.Abstractions.Events;
using BemfaCloud.Providers.Tcp.Abstractions.Listeners;
using System;
using System.Net;

namespace BemfaCloud.Providers.Tcp.Abstractions.Connections
{
    public interface IUdpConnection : IEvent<IUdpConnection, NetClientEventArgs<IUdpConnection>>, INetConnection, IDisposable
    {
        /// <summary>
        /// Gets the listener.
        /// </summary>
        /// <value>
        /// The listener.
        /// </value>
        IUdpListener Listener { get; }

        /// <summary>
        /// Gets the listen end point.
        /// </summary>
        /// <value>
        /// The listen end point.
        /// </value>
        IPEndPoint ListenEndPoint { get; }
    }
}