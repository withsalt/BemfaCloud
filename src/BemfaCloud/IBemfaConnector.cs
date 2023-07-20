using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BemfaCloud.Models;

namespace BemfaCloud
{
    public interface IBemfaConnector : IDisposable
    {
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// 是否已经连接到服务器
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 启动连接
        /// </summary>
        /// <returns></returns>
        Task<bool> ConnectAsync();

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        Task<bool> DisconnectAsync();

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="deviceTopic">主题</param>
        /// <param name="payload">消息负载</param>
        /// <remarks>
        /// 所有订阅这个主题的设备们推送消息，假如推送者自己也订阅了这个主题，消息不会被推送给它自己，以防止自己推送的消息被自己接收。
        /// </remarks>
        /// <returns></returns>
        Task PublishAsync(string deviceTopic, string payload);

        /// <summary>
        /// 更新云端数据
        /// </summary>
        /// <param name="deviceTopic"></param>
        /// <param name="payload"></param>
        /// <remarks>
        /// 只更新云端数据，不进行任何推送。
        /// </remarks>
        /// <returns></returns>
        Task UpdateAsync(string deviceTopic, string payload);

        /// <summary>
        /// 注册监听器（可以自行注册消息监听）
        /// </summary>
        /// <param name="action"></param>
        void RegistListener(Action<MessageEventArgs> action);
    }
}
