using System;
using System.Collections.Generic;
using System.Text;

namespace BemfaCloud.Exceptions
{
    public class DisconnectException : Exception
    {
        public DisconnectException()
        {

        }

        public DisconnectException(string message) : base(message)
        {

        }

        public DisconnectException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
