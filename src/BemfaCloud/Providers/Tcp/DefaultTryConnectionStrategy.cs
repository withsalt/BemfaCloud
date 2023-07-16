using BemfaCloud.Providers.Tcp.Abstractions.Components;

namespace BemfaCloud.Providers.Tcp
{
    internal class DefaultTryConnectionStrategy : ITryConnectionStrategy
    {
        public int Handle(int counter)
        {
            return (counter % 20) * 3000;
        }
    }
}