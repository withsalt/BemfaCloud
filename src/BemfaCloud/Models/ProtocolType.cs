using System;
using System.Collections.Generic;
using System.Text;

namespace BemfaCloud.Models
{
    public enum ProtocolType
    {
        /// <summary>
        /// MQTT
        /// </summary>
        Mqtt = 1,

        /// <summary>
        /// TCP
        /// </summary>
        Tcp = 3,

        /// <summary>
        /// MQTTV2
        /// </summary>
        Mqtt_V2 = 5,

        /// <summary>
        /// TCPV2
        /// </summary>
        Tcp_V2 = 7,

    }
}
