using System;
using BemfaCloud.Devices.Extensions;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    public class BemfaCurtain : BaseDevice
    {
        public override DeviceType DeviceType => DeviceType.Curtain;

        public event Func<MessageEventArgs, int, bool> On;
        public event Func<MessageEventArgs, int, bool> Off;
        public event Func<MessageEventArgs, bool> Pause;
        public event Action<Exception> OnException;

        private int _lastPercentage = 0;

        public BemfaCurtain(string topic, IBemfaConnector connector) : base(topic, connector)
        {

        }

        protected override bool Resolver(MessageEventArgs message)
        {
            string cmdStr = message.ToString();
            if (!cmdStr.StartsWith("on", StringComparison.OrdinalIgnoreCase)
                && !cmdStr.StartsWith("off", StringComparison.OrdinalIgnoreCase)
                && !cmdStr.StartsWith("pause", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            DeviceStatus status = DeviceStatus.Unknown;
            int percentage = _lastPercentage;

            string[] cmdArg = cmdStr.Split('#');
            for (int i = 0; i < cmdArg.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        switch (cmdArg[i].ToLower())
                        {
                            case "on": status = DeviceStatus.On; break;
                            case "off": status = DeviceStatus.Off; break;
                            case "pause": DevicePause(message); return true;
                            default: return false;
                        }
                        break;
                    case 1:
                        if (!int.TryParse(cmdArg[i], out percentage) || percentage < 0 || percentage > 100)
                        {
                            return false;
                        }
                        break;
                }
            }
            if (cmdArg.Length == 1)
            {
                percentage = status == DeviceStatus.On ? 100 : 0;
            }

            if (status == DeviceStatus.On)
                DeviceOn(message, percentage);
            else
                DeviceOff(message, percentage);
            return true;
        }

        /// <summary>
        /// 设备动作：开
        /// </summary>
        /// <param name="message"></param>
        private void DeviceOn(MessageEventArgs message, int percentage)
        {
            try
            {
                bool? result = On?.Invoke(message, percentage);
                if (result == null) return;
                if (result == true)
                {
                    this._lastPercentage = percentage;
                    this.DeviceStatus = DeviceStatus.On;
                }
                this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), _lastPercentage.ToString()));
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }

        /// <summary>
        /// 设备动作：关
        /// </summary>
        /// <param name="message"></param>
        private void DeviceOff(MessageEventArgs message, int percentage)
        {
            try
            {
                bool? result = Off?.Invoke(message, percentage);
                if (result == null) return;
                if (result == true)
                {
                    _lastPercentage = percentage;
                    this.DeviceStatus = DeviceStatus.Off;
                }
                this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), _lastPercentage.ToString()));
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }

        /// <summary>
        /// 设备动作：暂停
        /// </summary>
        /// <param name="message"></param>
        private void DevicePause(MessageEventArgs message)
        {
            Pause?.Invoke(message);
        }
    }
}
