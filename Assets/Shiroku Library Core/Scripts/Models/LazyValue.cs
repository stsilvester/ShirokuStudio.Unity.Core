using System;

namespace ShirokuStudio.Core
{
    public class LazyValue<T>
    {
        public T Value
        {
            get
            {
                _value ??= _factory();
                return _value;
            }
        }

        private T _value;

        private readonly Func<T> _factory;

        public LazyValue(Func<T> factory)
        {
            _factory = factory;
        }

        public static implicit operator T(LazyValue<T> lazyValue)
        {
            return lazyValue.Value;
        }
    }
}