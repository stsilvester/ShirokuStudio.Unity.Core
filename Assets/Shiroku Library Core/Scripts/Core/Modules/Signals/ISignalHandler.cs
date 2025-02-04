using System.Collections.Generic;
using System.Linq;

namespace ShirokuStudio.Siganls
{
    public interface ISignalHandler
    { }
}

namespace ShirokuStudio.Siganls
{
    public interface ISignalHandler<TSignal> : ISignalHandler
        where TSignal : ISignal
    {
        void HandleSignal(TSignal signal);
    }
}

namespace ShirokuStudio.Siganls
{
    public interface ISignalHandlerCollection
    {
        IEnumerable<ISignalHandler> SignalHandlers { get; }

        public void HandleSignal<TSignal>(TSignal signal)
            where TSignal : ISignal
            => GetSignalHandlers<TSignal>().Foreach(e => e.HandleSignal(signal));

        protected IEnumerable<ISignalHandler<T>> GetSignalHandlers<T>()
            where T : ISignal
            => SignalHandlers.OfType<ISignalHandler<T>>();
    }
}