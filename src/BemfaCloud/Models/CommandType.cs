using System;
using System.Collections.Generic;
using System.Text;

namespace BemfaCloud.Models
{
    public enum CommandType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknow = -1,

        /// <summary>
        /// 心跳
        /// </summary>
        Ping = 0,

        /// <summary>
        /// 订阅
        /// </summary>
        Subscribe = 1,

        /// <summary>
        /// 发送
        /// </summary>
        Publish = 2,

        /// <summary>
        /// 订阅并拉取一条消息
        /// </summary>
        SubAndPull = 3,

        /// <summary>
        /// 获取时间戳
        /// </summary>
        GetTimestamp = 7,

        /// <summary>
        /// 获取遗嘱消息
        /// </summary>
        TestamentMsg = 9
    }
}
