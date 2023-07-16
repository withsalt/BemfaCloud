using System;
using System.Collections.Generic;
using System.Text;
using BemfaCloud.Devices.Extensions;
using BemfaCloud.Devices.Models;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    public class BemfaSensor : BaseBemfaDevice
    {
        public override DeviceType DeviceType => DeviceType.Sensor;

        public BemfaSensor(string topic, IBemfaConnector connector) : base(topic, connector)
        {

        }

        protected override bool Resolver(MessageEventArgs message)
        {

            return true;
        }

        /// <summary>
        /// 设置温度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithTemperature(double value)
        {

            return this;
        }

        /// <summary>
        /// 设置湿度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithHumidity(double value)
        {

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
            return this;
        }

        /// <summary>
        /// 空气质量
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithAQI(int value)
        {

            return this;
        }

        /// <summary>
        /// 二氧化碳浓度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BemfaSensor WithPPM(int value)
        {

            return this;
        }

        /// <summary>
        /// 发送状态更新
        /// </summary>
        public void Update()
        {
            string cmd = CommandBuilder(this.DeviceStatus.GetDescription());
            this.Connector.UpdateAsync(this.DeviceInfo.Topic, cmd);
        }
    }
}
