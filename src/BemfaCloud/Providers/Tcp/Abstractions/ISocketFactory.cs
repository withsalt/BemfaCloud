using System.Net;
using System.Net.Sockets;
using BemfaCloud.Providers.Tcp.Abstractions.Components;

namespace BemfaCloud.Providers.Tcp.Abstractions
{
    internal interface ISocketFactory
    {
        /// <summary>
        /// Creates the TCP client.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        ITcpClient CreateTcpClient(TcpClientOptions options);

        /// <summary>
        /// Creates the TCP client.
        /// </summary>
        /// <param name="remoteHost">The remote host.</param>
        /// <param name="remotePort">The remote port.</param>
        /// <param name="localPort">The local port.</param>
        /// <param name="localHost">The local host.</param>
        /// <param name="connectTimeout">The connect timeout.</param>
        /// <param name="name">The name.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        ITcpClient CreateTcpClient(string remoteHost, int remotePort, int localPort = 0, string localHost = null, int connectTimeout = 3000, string name = null, int id = 0);

    }
}
