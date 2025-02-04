using System;
using System.Collections.Generic;

namespace ShirokuStudio.Core.Models
{
    public interface IDisposableCollection : ICollection<IDisposable>, IDisposable
    {
    }

    public class DisposableCollection : List<IDisposable>, IDisposableCollection
    {
        public void Dispose()
        {
            foreach (IDisposable disposable in this)
                disposable.Dispose();

            Clear();
        }
    }
}