using System;
using System.Collections.Generic;
using System.Text;

namespace BemfaCloud.Models
{
    public enum DateTimeType
    {
        /// <summary>
        /// 获取当前日期和时间，例如：2021-06-11 17:20:54
        /// </summary>
        DateTime = 1,

        /// <summary>
        /// 获取当前时间，例如：17:20:54
        /// </summary>
        Time = 2,

        /// <summary>
        /// 获取当前时间戳，例如：1623403325
        /// </summary>
        Timestamp = 3,
    }
}
