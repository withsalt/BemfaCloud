using System;
using System.Drawing;
using BemfaCloud.Devices.Extensions;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    public class BemfaFan : BaseBemfaDevice
    {
        public override DeviceType DeviceType => DeviceType.Fan;

        public event Func<MessageEventArgs, int, bool, bool> On;
        public event Func<MessageEventArgs, bool> Off;

        private int _lastLevel = 1;
        private bool _lastHeadStatus = false;

        public BemfaFan(string topic, IBemfaConnector connector) : base(topic, connector)
        {

        }

        protected override bool Resolver(MessageEventArgs message)
        {
            string cmdStr = message.ToString();
            if (!cmdStr.StartsWith("on", StringComparison.OrdinalIgnoreCase)
                && !cmdStr.StartsWith("off", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            string[] cmdArg = cmdStr.Split('#');
            DeviceStatus status = DeviceStatus.Unknown;
            int level = _lastLevel;
            bool headStatusVal = _lastHeadStatus;
            for (int i = 0; i < cmdArg.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        status = cmdArg[i].Equals("on", StringComparison.OrdinalIgnoreCase) ? DeviceStatus.On : DeviceStatus.Off;
                        break;
                    case 1:
                        if (!int.TryParse(cmdArg[i], out level) || level < 0 || level > 30)
                        {
                            return false;
                        }
                        if (level == 0)
                        {
                            status = DeviceStatus.Off;
                            level = 1;
                        }
                        break;
                    case 2:
                        if (!int.TryParse(cmdArg[i], out int headStatusIntVal))
                        {
                            return false;
                        }
                        headStatusVal = headStatusIntVal == 1;
                        break;
                }
            }
            if (status == DeviceStatus.On)
            {
                DeviceOn(message, level, headStatusVal);
            }
            else
            {
                DeviceOff(message);
            }
            return true;
        }

        /// <summary>
        /// 设备动作：开
        /// </summary>
        /// <param name="message"></param>
        private void DeviceOn(MessageEventArgs message, int level, bool headStatus)
        {
            bool? result = On?.Invoke(message, level, headStatus);
            if (result == null) return;
            if (result == true)
            {
                _lastHeadStatus = headStatus;
                _lastLevel = level;
                this.DeviceStatus = DeviceStatus.On;
            }
            this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), _lastLevel.ToString(), _lastHeadStatus ? "1" : "0"));
        }

        /// <summary>
        /// 设备动作：关
        /// </summary>
        /// <param name="message"></param>
        private void DeviceOff(MessageEventArgs message)
        {
            bool? result = Off?.Invoke(message);
            if (result == null) return;
            if (result == true)
            {
                this.DeviceStatus = DeviceStatus.Off;
            }
            this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), _lastLevel.ToString(), _lastHeadStatus ? "1" : "0"));
        }
    }
}
