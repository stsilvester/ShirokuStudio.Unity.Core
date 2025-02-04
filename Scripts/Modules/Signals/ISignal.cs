namespace ShirokuStudio.Siganls
{
    public interface ISignal
    {
    }
}

namespace ShirokuStudio.Siganls
{
    public interface ITransmitSignal : ISignal
    {
        ISignalHandlerCollection TransmitTarget { get; }
    }
}