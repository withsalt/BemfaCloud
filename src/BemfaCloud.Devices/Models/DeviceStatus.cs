using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BemfaCloud.Devices.Models
{
    public enum DeviceStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        [Description("unknown")]
        Unknown = 0,

        /// <summary>
        /// 开启
        /// </summary>
        [Description("on")]
        On = 1,

        /// <summary>
        /// 关闭
        /// </summary>
        [Description("off")]
        Off = 2
    }
}
