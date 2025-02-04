using UniRx;

namespace ShirokuStudio.Core
{
    public interface ISelectable
    {
        BoolReactiveProperty IsSelected { get; }
        BoolReactiveProperty IsEnabled { get; }
    }
}