using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BemfaCloud.Devices.Extensions;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    public class BemfaSensor : BaseDevice
    {
        public override DeviceType DeviceType => DeviceType.Sensor;

        public event Func<MessageEventArgs, bool> On;
        public event Func<MessageEventArgs, bool> Off;
        public event Action<Exception> OnException;

        public BemfaSensor(string topic, IBemfaConnector connector) : base(topic, connector)
        {
            this.DeviceStatus = DeviceStatus.On;
        }

        private ConcurrentDictionary<int, string> _container = null;
        private static readonly object _locker = new object();

        private void DestroyDataContainer()
        {
            if (_container == null) return;
            lock (_locker)
            {
                if (_container == null) return;

                _container.Clear();
                _container = null;
            }
        }

        /// <summary>
        /// #温度#湿度#开关#光照#pm2.5#心率
        /// </summary>
        /// <returns></returns>
        private ConcurrentDictionary<int, string> GetDataContainer()
        {
            if (_container != null) return _container;
            lock (_locker)
            {
                if (_container != null) return _container;

                _container = new ConcurrentDictionary<int, string>();
                return _container;
            }
        }

        protected override bool Excute(MessageEventArgs message)
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
                if (result == true) 
                    this.DeviceStatus = DeviceStatus.On;

                //传感器不返回电源【开】状态
                //this.WithDeviceStatus(DeviceStatus.On).Update();
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
                if (result == true) this.DeviceStatus = DeviceStatus.Off;

                this.WithDeviceStatus(DeviceStatus.Off).Update();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }

        /// <summary>
        /// 设置温度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithTemperature(double value)
        {
            int index = 0;
            ConcurrentDictionary<int, string> container = GetDataContainer();
            container[index] = value.ToString("0.##");
            return this;
        }

        /// <summary>
        /// 设置湿度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithHumidity(double value)
        {
            int index = 1;
            ConcurrentDictionary<int, string> container = GetDataContainer();
            container[index] = value.ToString("0.##");
            return this;
        }

        /// <summary>
        /// 电源状态
        /// </summary>
        /// <param name="value">ON/OFF</param>
        /// <returns></returns>
        public BemfaSensor WithDeviceStatus(DeviceStatus value)
        {
            if (value != DeviceStatus.On && value != DeviceStatus.Off)
            {
                throw new ArgumentException(nameof(value), "Device status only support On or Off.");
            }
            this.DeviceStatus = value;

            int index = 2;
            ConcurrentDictionary<int, string> container = GetDataContainer();
            container[index] = value.GetDescription();
            return this;
        }

        /// <summary>
        /// 空气质量
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithAQI(int value)
        {
            int index = 3;
            ConcurrentDictionary<int, string> container = GetDataContainer();
            container[index] = value.ToString("0.##");
            return this;
        }

        /// <summary>
        /// 光照强度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithLux(int value)
        {
            int index = 3;
            ConcurrentDictionary<int, string> container = GetDataContainer();
            container[index] = value.ToString("0.##");
            return this;
        }

        /// <summary>
        /// PM2.5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithPM25(double value)
        {
            int index = 4;
            ConcurrentDictionary<int, string> container = GetDataContainer();
            container[index] = value.ToString("0.##");
            return this;
        }

        /// <summary>
        /// 心率
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithHeartRate(int value)
        {
            int index = 4;
            ConcurrentDictionary<int, string> container = GetDataContainer();
            container[index] = value.ToString("0.##");
            return this;
        }

        /// <summary>
        /// 二氧化碳浓度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithPPM(double value)
        {
            int index = 5;
            ConcurrentDictionary<int, string> container = GetDataContainer();
            container[index] = value.ToString("0.##");
            return this;
        }

        /// <summary>
        /// 发送状态更新
        /// </summary>
        public void Update()
        {
            try
            {
                ConcurrentDictionary<int, string> container = GetDataContainer();
                if (container?.Any() != true)
                {
                    return;
                }
                int maxIndex = container.Keys.Max() + 1;
                //定义需要发送数据的数组
                string[] values = new string[maxIndex];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = "";
                }
                //将需要发送的数据放入数组对应的插槽中
                var containerArgs = container.ToArray();
                foreach (var item in containerArgs)
                {
                    values[item.Key] = item.Value;
                }
                string cmd = "#" + CommandBuilder(values);
                this.Connector.UpdateAsync(this.DeviceInfo.Topic, cmd);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
            finally
            {
                DestroyDataContainer();
            }
        }
    }
}
