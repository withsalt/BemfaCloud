using System;
using System.Collections.Generic;
using System.Linq;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    public abstract class BaseDevice
    {
        public IBemfaConnector Connector { get; private set; }

        public DeviceInfo DeviceInfo { get; private set; }

        public DeviceStatus DeviceStatus { get; protected set; }

        public abstract DeviceType DeviceType { get; }

        /// <summary>
        /// 除了能够识别的控制命令以外的其他类型消息
        /// </summary>
        public event Action<MessageEventArgs> OnMessage;

        private readonly Dictionary<CommandType, int> _skipCommandType = new Dictionary<CommandType, int>()
        {
            { CommandType.Unknow, (int)CommandType.Unknow },
            { CommandType.Ping, (int)CommandType.Ping },
            { CommandType.GetTimestamp, (int)CommandType.GetTimestamp }
        };

        protected abstract bool Excute(MessageEventArgs message);

        public BaseDevice(string topic, IBemfaConnector connector)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentNullException(nameof(topic));
            }
            this.DeviceInfo = new DeviceInfo(topic);
            this.Connector = connector ?? throw new ArgumentNullException(nameof(connector));
            this.Connector.MessageEventRegister(MessageEventHandle);
        }

        protected virtual string CommandBuilder(params string[] cmds)
        {
            if (cmds?.Any() != true)
            {
                throw new ArgumentException("Command args can not null.");
            }
            return string.Join("#", cmds);
        }

        private void MessageEventHandle(MessageEventArgs message)
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
            if (_skipCommandType.ContainsKey(message.Type))
            {
                return;
            }
            if (!this.DeviceInfo.Equals(message.DeviceInfo))
            {
                return;
            }
            bool isSuccess = Excute(message);
            if (!isSuccess)
            {
                OnMessage?.Invoke(message);
            }
        }
    }
}
