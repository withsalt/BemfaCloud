using System;
using System.Collections.Generic;
using System.Text;

namespace BemfaCloud.Models
{
    public enum DeviceType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknow = 0,

        /// <summary>
        /// 插座
        /// </summary>
        Socket = 1,

        /// <summary>
        /// 灯
        /// </summary>
        Light = 2,

        /// <summary>
        /// 风扇
        /// </summary>
        Fan = 3,

        /// <summary>
        /// 传感器
        /// </summary>
        Sensor = 4,

        /// <summary>
        /// 空调
        /// </summary>
        AirConditioning = 5,

        /// <summary>
        /// 开关
        /// </summary>
        Switch = 6,

        /// <summary>
        /// 窗帘
        /// </summary>
        Curtain = 9,
    }
}
