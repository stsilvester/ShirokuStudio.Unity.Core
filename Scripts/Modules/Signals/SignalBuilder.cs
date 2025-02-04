using System;

namespace ShirokuStudio.Siganls
{
    public interface ISignalBuilder
    {
    }

    public interface ISignalBuilder<TSignal> : ISignalBuilder, IDisposable
    {
        TSignal Populate();
    }
}