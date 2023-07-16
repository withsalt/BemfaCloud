using System;
using System.Collections.Generic;
using System.Text;

namespace BemfaCloud.Models
{
    public class DeviceInfo
    {
        public DeviceInfo(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentNullException("The topic can not null.");
            }
            if (topic.Length <= 3)
            {
                this.Topic = topic;
                this.DeviceType = DeviceType.Unknow;
                return;
            }
            string typeStr = topic.Substring(topic.Length - 3);
            if (int.TryParse(typeStr, out int typeInt) && Enum.IsDefined(typeof(DeviceType), typeInt))
            {
                this.Topic = topic;
                this.DeviceType = (DeviceType)typeInt;
                return;
            }
            else
            {
                this.Topic = topic;
                this.DeviceType = DeviceType.Unknow;
                return;
            }
        }

        /// <summary>
        /// Topic
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public DeviceType DeviceType { get; private set; }

        /// <summary>
        /// 判断是否同一个device
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(DeviceInfo b)
        {
            if (b == null) return false;
            return this.Topic.Equals(b.Topic, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断是否同一个device
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return false;
            return this.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase);
        }

    }
}
