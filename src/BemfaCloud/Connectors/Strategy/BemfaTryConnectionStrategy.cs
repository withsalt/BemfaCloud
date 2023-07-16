using System;
using System.Collections.Generic;
using System.Text;
using BemfaCloud.Connectors.Builder;
using BemfaCloud.Providers.Tcp.Abstractions.Components;

namespace BemfaCloud.Connectors.Strategy
{
    internal class BemfaTryConnectionStrategy : ITryConnectionStrategy
    {
        private BaseConnectorBuilder Builder { get; set; }

        public BemfaTryConnectionStrategy(BaseConnectorBuilder builder) : base()
        {
            this.Builder = builder;
        }

        public int Handle(int counter)
        {
            return Builder.AutoReconnectDelaySeconds * 1000;
        }

        public bool IsEnableReconnect
        {
            get
            {
                return Builder != null && Builder.AutoReconnectDelaySeconds >= 0;
            }
        }

        public bool IsReconnectImmediately
        {
            get
            {
                return Builder != null && Builder.AutoReconnectDelaySeconds == 0;
            }
        }

        public int AutoReconnectDelaySeconds
        {
            get
            {
                if (Builder == null) return -1;
                return Builder.AutoReconnectDelaySeconds;
            }
        }
    }
}
