namespace BemfaCloud.Providers.Tcp.Abstractions.Components
{
    public interface ITryConnectionStrategy
    {
        int Handle(int counter);
    }
}
