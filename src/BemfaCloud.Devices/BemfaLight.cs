using System;
using System.Drawing;
using BemfaCloud.Devices.Extensions;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    public class BemfaLight : BaseDevice
    {
        public override DeviceType DeviceType => DeviceType.Light;

        public event Func<MessageEventArgs, int, Color, bool> On;
        public event Func<MessageEventArgs, bool> Off;
        public event Action<Exception> OnException;

        private int _lastBrightness = 100;
        private Color _lastColor = Color.White;

        public BemfaLight(string topic, IBemfaConnector connector) : base(topic, connector)
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
            int brightness = _lastBrightness;
            Color colorVal = _lastColor;
            for (int i = 0; i < cmdArg.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        status = cmdArg[i].Equals("on", StringComparison.OrdinalIgnoreCase) ? DeviceStatus.On : DeviceStatus.Off;
                        break;
                    case 1:
                        if (!int.TryParse(cmdArg[i], out brightness) || brightness < 1 || brightness > 100)
                        {
                            return false;
                        }
                        break;
                    case 2:
                        if (!int.TryParse(cmdArg[i], out int colorIntVal))
                        {
                            return false;
                        }
                        colorVal = ConvertInt32ToColor(colorIntVal);
                        break;
                }
            }
            if (status == DeviceStatus.On)
            {
                DeviceOn(message, brightness, colorVal);
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
        private void DeviceOn(MessageEventArgs message, int brightness, Color color)
        {
            try
            {
                bool? result = On?.Invoke(message, brightness, color);
                if (result == null) return;
                if (result == true)
                {
                    _lastColor = color;
                    _lastBrightness = brightness;
                    this.DeviceStatus = DeviceStatus.On;
                }
                this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), _lastBrightness.ToString(), ConvertColorToInt32(_lastColor).ToString()));
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
                this.Connector.UpdateAsync(message.DeviceInfo.Topic, CommandBuilder(this.DeviceStatus.GetDescription(), _lastBrightness.ToString(), ConvertColorToInt32(_lastColor).ToString()));
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }

        private Color ConvertInt32ToColor(int val)
        {
            return ColorTranslator.FromHtml("#" + Convert.ToString(val, 16));
        }

        private int ConvertColorToInt32(Color color)
        {
            string htmlColor = $"{color.R:X2}{color.G:X2}{color.B:X2}";
            return Convert.ToInt32(htmlColor.TrimStart('#'), 16);
        }
    }
}
