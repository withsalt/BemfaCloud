using System;
using System.Drawing;
using BemfaCloud.Devices.Extensions;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    public class BemfaAircon : BaseDevice
    {
        public override DeviceType DeviceType => DeviceType.AirConditioning;

        public event Func<MessageEventArgs, AirconMode, double, bool> On;
        public event Func<MessageEventArgs, bool> Off;
        public event Action<Exception> OnException;

        private AirconMode _lastAirconMode = AirconMode.Auto;
        private double _lastTemperature = 24.0;

        public BemfaAircon(string topic, IBemfaConnector connector) : base(topic, connector)
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
            AirconMode airconMode = _lastAirconMode;
            double temperatureVal = _lastTemperature;
            for (int i = 0; i < cmdArg.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        status = cmdArg[i].Equals("on", StringComparison.OrdinalIgnoreCase) ? DeviceStatus.On : DeviceStatus.Off;
                        break;
                    case 1:
                        if (!int.TryParse(cmdArg[i], out int airconModeInt) || !Enum.IsDefined(typeof(AirconMode), airconModeInt))
                        {
                            return false;
                        }
                        airconMode = (AirconMode)airconModeInt;
                        break;
                    case 2:
                        if (!double.TryParse(cmdArg[i], out temperatureVal))
                        {
                            return false;
                        }
                        temperatureVal = Math.Round(temperatureVal, 1);
                        break;
                }
            }
            if (status == DeviceStatus.On)
            {
                DeviceOn(message, airconMode, temperatureVal);
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
        private void DeviceOn(MessageEventArgs message, AirconMode airconMode, double temperature)
        {
            try
            {
                bool? result = On?.Invoke(message, airconMode, temperature);
                if (result == null) return;
                if (result == true)
                {
                    _lastTemperature = temperature;
                    _lastAirconMode = airconMode;
                    this.DeviceStatus = DeviceStatus.On;
                }
                this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), ((int)_lastAirconMode).ToString(), _lastTemperature.ToString()));
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
                if (result == true)
                {
                    this.DeviceStatus = DeviceStatus.Off;
                }
                this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), ((int)_lastAirconMode).ToString(), _lastTemperature.ToString()));
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }
    }
}
