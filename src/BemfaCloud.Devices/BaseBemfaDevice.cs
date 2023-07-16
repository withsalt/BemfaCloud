﻿using System;
using System.Linq;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    public abstract class BaseBemfaDevice
    {
        public IBemfaConnector Connector { get; private set; }

        public DeviceInfo DeviceInfo { get; private set; }

        public DeviceStatus DeviceStatus { get; protected set; }

        public abstract DeviceType DeviceType { get; }

        /// <summary>
        /// 除了能够识别的控制命令以外的其他类型消息
        /// </summary>
        public event Action<MessageEventArgs> OnMessage;

        protected abstract bool Resolver(MessageEventArgs message);

        public BaseBemfaDevice(string topic, IBemfaConnector connector)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentNullException(nameof(topic));
            }
            this.DeviceInfo = new DeviceInfo(topic);
            this.Connector = connector ?? throw new ArgumentNullException(nameof(connector));
            this.Connector.RegistListener(Listener);
        }

        protected virtual string CommandBuilder(params string[] cmds)
        {
            if (cmds?.Any() != true)
            {
                throw new ArgumentException("Command args can not null.");
            }
            return string.Join("#", cmds);
        }

        private void Listener(MessageEventArgs message)
        {
            if (message == null || message.Data.Array?.Any() != true)
            {
                return;
            }
            string cmdStr = message.ToString();
            if (string.IsNullOrWhiteSpace(cmdStr))
            {
                return;
            }
            if (!this.DeviceInfo.Equals(message.DeviceInfo))
            {
                return;
            }
            bool isSuccess = Resolver(message);
            if (!isSuccess)
            {
                OnMessage?.Invoke(message);
            }
        }
    }
}
