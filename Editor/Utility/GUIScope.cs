using System;
using System.Collections.Generic;

namespace ShirokuStudio.Editor
{
    public class GUIScope : IDisposable
    {
        private readonly Action onDispose;
        private readonly List<IDisposable> disposables = new();

        public GUIScope(Action dispose)
        {
            onDispose = dispose;
        }

        public GUIScope Append(params IDisposable[] otherDisposable)
        {
            disposables.AddRange(otherDisposable);
            return this;
        }

        public void Dispose()
        {
            onDispose.Invoke();
            foreach (var item in disposables)
                item.Dispose();
            disposables.Clear();
        }
    }
}

namespace CommonEditor
{
}