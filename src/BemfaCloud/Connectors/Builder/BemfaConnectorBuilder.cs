using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BemfaCloud.Models;

namespace BemfaCloud.Connectors.Builder
{
    public class BemfaConnectorBuilder : BaseConnectorBuilder
    {
        public BemfaConnectorBuilder WithTcp()
        {
            ProtocolType = ProtocolType.Tcp;
            Host = "bemfa.com";
            Port = 8344;
            if (IsEnableTls)
            {
                return WithTls();
            }
            return this;
        }

        public BemfaConnectorBuilder WithMqtt()
        {
            ProtocolType = ProtocolType.Mqtt;
            Host = "bemfa.com";
            Port = 9501;
            if (IsEnableTls)
            {
                return WithTls();
            }
            return this;
        }

        public BemfaConnectorBuilder WithTls(string fileName = null, string password = null)
        {
            switch (this.ProtocolType)
            {
                case ProtocolType.Tcp:
                case ProtocolType.Tcp_V2:
                    {
                        ProtocolType = ProtocolType.Tcp;
                        Host = "bemfa.com";
                        Port = 8344;
                        IsEnableTls = true;
                    }
                    break;
                case ProtocolType.Mqtt:
                case ProtocolType.Mqtt_V2:
                    {
                        ProtocolType = ProtocolType.Mqtt;
                        Host = "bemfa.com";
                        Port = 9503;
                        IsEnableTls = true;
                    }
                    break;
            }
            return this;
        }

        public BemfaConnectorBuilder WithSecret(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "The secret key can not null");
            }
            Secret = key;
            return this;
        }

        public BemfaConnectorBuilder WithTopics(params string[] topics)
        {
            if (topics?.Any() != true)
            {
                throw new ArgumentException("The number of topics cannot null or zero.");
            }
            if (topics?.Length > 8)
            {
                throw new ArgumentException("The number of topics cannot more than 8.");
            }
            foreach (string topic in topics)
            {
                Devices.Add(new DeviceInfo(topic));
            }
            return this;
        }

        /// <summary>
        /// 自动重连间隔时间，单位为：秒，默认值为3s。如果小于0，则关闭自动重连。
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public BemfaConnectorBuilder WithAutoReconnectDelay(int seconds)
        {
            this.AutoReconnectDelaySeconds = seconds;
            return this;
        }

        public BemfaConnectorBuilder WithMessageHandler(Action<MessageEventArgs> action)
        {
            if (action == null)
                return this;

            this.OnMessageHandler = action;
            return this;
        }

        public BemfaConnectorBuilder WithErrorHandler(Action<ErrorEventArgs> action)
        {
            if (action == null)
                return this;

            this.OnErrorHandler = action;
            return this;
        }

        public IBemfaConnector Build()
        {
            switch (this.ProtocolType)
            {
                case ProtocolType.Tcp:
                    return new TcpConnector(this);
                case ProtocolType.Mqtt:
                    return new MqttConnector(this);
            }
            throw new NotImplementedException($"Unsupport protocol type: {this.ProtocolType}");
        }
    }
}
