using UnityEngine.UIElements;

namespace ShirokuStudio.Core
{
    public static class VisualElementExtensions
    {
        public static void ToggleClass(this VisualElement e, string className, bool toggle)
        {
            if (toggle)
            {
                e.AddToClassList(className);
            }
            else
            {
                e.RemoveFromClassList(className);
            }
        }
    }
}