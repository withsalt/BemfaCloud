using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BemfaCloud.Models
{
    public class MessageEventArgs
    {
        private string _message = null;

        internal MessageEventArgs(CommandType commandType, DeviceInfo deviceInfo, ArraySegment<byte> data)
        {
            this.Type = commandType;
            this.DeviceInfo = deviceInfo;
            this.Data = data;
        }

        public CommandType Type { get; internal set; }

        public DeviceInfo DeviceInfo { get; internal set; }

        public ArraySegment<byte> Data { get; internal set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(_message))
            {
                return _message;
            }
            if (Data == null)
            {
                return null;
            }
            if (Data.Any() != true)
            {
                return "";
            }
            _message = Encoding.UTF8.GetString(Data);
            return _message;
        }
    }
}
