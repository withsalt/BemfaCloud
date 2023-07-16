
using System;
using System.Collections.Generic;
using System.Text;

namespace BemfaCloud.Models
{
    public class ErrorEventArgs
    {
        public ErrorEventArgs()
        {

        }

        public ErrorEventArgs(string message)
        {
            this.LogType = LogType.Info;
            this.Message = message;
        }

        public ErrorEventArgs(LogType logType, string message)
        {
            this.LogType = logType;
            this.Message = message;
            if(logType == LogType.Error)
            {
                this.Exception = new Exception(message);
            }
        }

        public ErrorEventArgs(LogType logType, string message, Exception exception)
        {
            this.LogType = logType;
            this.Message = message;
            this.Exception = exception;
        }

        public LogType LogType { get; internal set; }

        public string Message { get; internal set; }

        public Exception Exception { get; internal set; }
    }
}
