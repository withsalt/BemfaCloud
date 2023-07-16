﻿using System;
using System.Net;
using BemfaCloud.Providers.Tcp.Abstractions.Components;
using BemfaCloud.Providers.Tcp.Abstractions.Connections;

namespace BemfaCloud.Providers.Tcp.Abstractions.Events
{
    /// <summary>
    /// NetEventArgs
    /// </summary>
    public abstract class NetEventArgs
    {
        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; internal set; }
    }

    /// <summary>
    /// NetClientEventArgs
    /// </summary>
    /// <seealso cref="NetEventArgs" />
    public class NetClientEventArgs<TConnection> : NetEventArgs where TConnection : INetConnection
    {
        /// <summary>
        /// Gets the local endpoint of a Transmission Control Protocol (TCP) connection.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; internal set; }

        /// <summary>
        /// Gets the remote endpoint of a Transmission Control Protocol (TCP) connection.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; internal set; }

        /// <summary>
        /// Gets the net connection.
        /// </summary>
        public TConnection NetConnection { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetClientEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public NetClientEventArgs(TConnection connection)
        {
            NetConnection = connection;
            RemoteEndPoint = connection?.RemoteEndPoint;
            LocalEndPoint = connection?.LocalEndPoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetClientEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="remote">The remote.</param>
        /// <param name="local">The local.</param>
        public NetClientEventArgs(TConnection connection, IPEndPoint remote = null, IPEndPoint local = null) : this(connection)
        {
            if (remote != null) RemoteEndPoint = remote;
            if (local != null) LocalEndPoint = local;
        }
    }

    /// <summary>
    /// NetClientReceivedEventArgs
    /// </summary>
    /// <seealso cref="GodSharp.Sockets.NetClientEventArgs" />
    public class NetClientReceivedEventArgs<TConnection> : NetClientEventArgs<TConnection> where TConnection : INetConnection
    {
        public byte[] Buffers { get; internal set; }

        public NetClientReceivedEventArgs(TConnection connection, byte[] buffers) : this(connection, buffers, null, null)
        {
        }

        public NetClientReceivedEventArgs(TConnection connection, byte[] buffers, IPEndPoint remote = null, IPEndPoint local = null) : base(connection, remote, local)
        {
            Buffers = buffers;
        }
    }

    /// <summary>
    /// NetServerEventArgs
    /// </summary>
    /// <seealso cref="NetEventArgs" />
    public class NetServerEventArgs : NetEventArgs
    {
        /// <summary>
        /// Gets the local end point.
        /// </summary>
        /// <value>
        /// The local end point.
        /// </value>
        public IPEndPoint LocalEndPoint { get; internal set; }

        /// <summary>
        /// Gets the TCP server.
        /// </summary>
        /// <value>
        /// The TCP server.
        /// </value>
        public ITcpServer TcpServer { get; internal set; }

        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public ITcpConnection Connection { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetServerEventArgs"/> class.
        /// </summary>
        public NetServerEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetServerEventArgs"/> class.
        /// </summary>
        /// <param name="tcpServer">The TCP server.</param>
        /// <param name="localEndPoint">The local end point.</param>
        /// <param name="connection">The connection.</param>
        public NetServerEventArgs(ITcpServer tcpServer, IPEndPoint localEndPoint = null, ITcpConnection connection = null)
        {
            if (tcpServer != null) TcpServer = tcpServer;
            if (localEndPoint != null) LocalEndPoint = localEndPoint;
            if (connection != null) Connection = connection;
        }
    }

    /// <summary>
    /// TryConnectingEventArgs
    /// </summary>
    /// <seealso cref="GodSharp.Sockets.NetClientEventArgs" />
    public class TryConnectingEventArgs<TConnection> : NetClientEventArgs<TConnection> where TConnection : INetConnection
    {
        public int Counter { get; internal set; }

        public TryConnectingEventArgs(TConnection connection, int counter) : this(connection, counter, null, null)
        {
        }

        public TryConnectingEventArgs(TConnection connection, int counter, IPEndPoint remote = null, IPEndPoint local = null) : base(connection, remote, local)
        {
            Counter = counter;
        }
    }
}
