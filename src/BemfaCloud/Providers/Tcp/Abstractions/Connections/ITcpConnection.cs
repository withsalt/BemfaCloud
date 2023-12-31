﻿using BemfaCloud.Providers.Tcp.Abstractions.Events;
using BemfaCloud.Providers.Tcp.Abstractions.Listeners;
using System;

namespace BemfaCloud.Providers.Tcp.Abstractions.Connections
{
    public interface ITcpConnection : IEvent<ITcpConnection, NetClientEventArgs<ITcpConnection>>, INetConnection, IDisposable
    {
        /// <summary>
        /// Gets the connect timeout.
        /// </summary>
        /// <value>
        /// The connect timeout.
        /// </value>
        int ConnectTimeout { get; }

        /// <summary>
        /// socket is connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets the listener.
        /// </summary>
        /// <value>
        /// The listener.
        /// </value>
        ITcpListener Listener { get; }
    }
}