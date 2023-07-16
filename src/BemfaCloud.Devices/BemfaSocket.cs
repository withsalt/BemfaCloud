using System;
using System.Collections.Generic;
using System.Text;
using BemfaCloud.Models;

namespace BemfaCloud.Devices
{
    /// <summary>
    /// 插座设备
    /// </summary>
    public class BemfaSocket : BemfaSwitch
    {
        public override DeviceType DeviceType => DeviceType.Socket;

        public BemfaSocket(string topic, IBemfaConnector connector) : base(topic, connector)
        {

        }
    }
}
