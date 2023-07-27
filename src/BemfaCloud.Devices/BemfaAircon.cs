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

        public event Func<MessageEventArgs, AirconMode, double, int, bool?, bool?, bool> On;
        public event Func<MessageEventArgs, bool> Off;
        public event Action<Exception> OnException;

        private AirconMode _lastAirconMode = AirconMode.Auto;
        private double _lastTemperature = 24.0;
        private int _lastLevel = 0;
        private bool? _lastIsLeftAndRightSweeping = null;
        private bool? _lastIsUpAndDownSweeping = null;

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

            DeviceStatus status = DeviceStatus.Unknown;
            AirconMode airconMode = _lastAirconMode;
            double temperatureVal = _lastTemperature;
            int levelVal = _lastLevel;
            bool? isLeftAndRightSweeping = _lastIsLeftAndRightSweeping;
            bool? isUpAndDownSweeping = _lastIsUpAndDownSweeping;

            string[] cmdArg = cmdStr.Split('#');
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
                    case 3:
                        if (!int.TryParse(cmdArg[i], out levelVal))
                        {
                            return false;
                        }
                        break;
                    case 4:
                        if (cmdArg[i] == "1")
                            isLeftAndRightSweeping = true;
                        else if (cmdArg[i] == "0")
                            isLeftAndRightSweeping = false;
                        else
                            isLeftAndRightSweeping = null;
                        break;
                    case 5:
                        if (cmdArg[i] == "1")
                            isUpAndDownSweeping = true;
                        else if (cmdArg[i] == "0")
                            isUpAndDownSweeping = false;
                        else
                            isUpAndDownSweeping = null;
                        break;
                }
            }
            if (status == DeviceStatus.On)
            {
                DeviceOn(message, airconMode, temperatureVal, levelVal, isLeftAndRightSweeping, isUpAndDownSweeping);
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
        private void DeviceOn(MessageEventArgs message
            , AirconMode airconMode
            , double temperature
            , int level
            , bool? isLeftAndRightSweeping
            , bool? isUpAndDownSweeping)
        {
            try
            {
                bool? result = On?.Invoke(message, airconMode, temperature, level, isLeftAndRightSweeping, isUpAndDownSweeping);
                if (result == null) return;
                if (result == true)
                {
                    _lastTemperature = temperature;
                    _lastAirconMode = airconMode;
                    _lastLevel = level;
                    _lastIsLeftAndRightSweeping = isLeftAndRightSweeping;
                    _lastIsUpAndDownSweeping = isUpAndDownSweeping;
                    this.DeviceStatus = DeviceStatus.On;
                }

                string cmdStr = null;
                if (_lastIsLeftAndRightSweeping == null && _lastIsUpAndDownSweeping == null)
                {
                    cmdStr = CommandBuilder(this.DeviceStatus.GetDescription()
                        , ((int)_lastAirconMode).ToString()
                        , _lastTemperature.ToString()
                        , _lastLevel.ToString());
                }
                else
                {
                    cmdStr = CommandBuilder(this.DeviceStatus.GetDescription()
                        , ((int)_lastAirconMode).ToString()
                        , _lastTemperature.ToString()
                        , _lastLevel.ToString()
                        , _lastIsLeftAndRightSweeping == null ? "" : (_lastIsLeftAndRightSweeping == true ? "1" : "0")
                        , _lastIsUpAndDownSweeping == null ? "" : (_lastIsUpAndDownSweeping == true ? "1" : "0"));
                }
                this.Connector.UpdateAsync(message.DeviceInfo.Topic, cmdStr);
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
                this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), ((int)_lastAirconMode).ToString(), _lastTemperature.ToString(), _lastLevel.ToString()));
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }
    }
}
