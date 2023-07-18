using System;
using BemfaCloud.Devices.Extensions;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    /// <summary>
    /// 开关设备
    /// </summary>
    public class BemfaSwitch : BaseDevice
    {
        public override DeviceType DeviceType => DeviceType.Switch;

        public event Func<MessageEventArgs, bool> On;
        public event Func<MessageEventArgs, bool> Off;
        public event Action<Exception> OnException;

        public BemfaSwitch(string topic, IBemfaConnector connector) : base(topic, connector)
        {

        }


        protected override bool Resolver(MessageEventArgs message)
        {
            switch (message.ToString().ToLower())
            {
                case "on":
                    DeviceOn(message);
                    break;
                case "off":
                    DeviceOff(message);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 设备动作：开
        /// </summary>
        /// <param name="message"></param>
        private void DeviceOn(MessageEventArgs message)
        {
            try
            {
                bool? result = On?.Invoke(message);
                if (result == null) return;
                if (result == true) this.DeviceStatus = Models.DeviceStatus.On;

                this.Connector.UpdateAsync(message.DeviceInfo.Topic, this.DeviceStatus.GetDescription());
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
        private void DeviceOff(MessageEventArgs message)
        {
            try
            {
                bool? result = Off?.Invoke(message);
                if (result == null) return;
                if (result == true) this.DeviceStatus = Models.DeviceStatus.Off;

                this.Connector.UpdateAsync(message.DeviceInfo.Topic, this.DeviceStatus.GetDescription());
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }
    }
}
