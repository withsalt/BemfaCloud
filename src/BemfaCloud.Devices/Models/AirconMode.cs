using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BemfaCloud.Devices.Models
{
    /// <summary>
    /// 空调模式
    /// </summary>
    public enum AirconMode
    {
        /// <summary>
        /// 自动
        /// </summary>
        [Description("1")]
        Auto = 1,

        /// <summary>
        /// 制冷
        /// </summary>
        [Description("2")]
        Cooling = 2,

        /// <summary>
        /// 制热
        /// </summary>
        [Description("3")]
        Heating = 3,

        /// <summary>
        /// 送风
        /// </summary>
        [Description("4")]
        Blowning = 4,

        /// <summary>
        /// 除湿
        /// </summary>
        [Description("5")]
        Dewatering = 5,

        /// <summary>
        /// 睡眠
        /// </summary>
        [Description("6")]
        Sleep = 6,

        /// <summary>
        /// 节能
        /// </summary>
        [Description("7")]
        EnergySaving = 7,
    }
}
