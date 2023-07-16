using System;
using System.Collections.Generic;
using System.Text;
using BemfaCloud.Models;

namespace BemfaCloud.Connectors.Builder
{
    public abstract class BaseConnectorBuilder
    {
        internal ProtocolType ProtocolType { get; set; } = ProtocolType.Mqtt;

        internal string Host { get; set; } = "bemfa.com";

        internal uint Port { get; set; } = 9501;

        internal string Secret { get; set; }

        internal List<DeviceInfo> Devices { get; } = new List<DeviceInfo>();

        internal bool IsEnableTls { get; set; } = false;

        internal int AutoReconnectDelaySeconds { get; set; } = 3;

        internal Action<MessageEventArgs> OnMessageHandler { get; set; } = null;

        internal Action<ErrorEventArgs> OnErrorHandler { get; set; } = null;

    }
}
