namespace ShirokuStudio.Core
{
    public interface IValueInput
    {
        void SetValue(object value);
    }

    public interface IValueInput<T> : IValueInput
    {
        void SetValue(T value);
    }

    public interface IValueCollectionInput
    {
        void SetValues(object[] values);
    }

    public interface IValueCollectionInput<T> : IValueCollectionInput
    {
        void SetValues(T[] values);
    }
}