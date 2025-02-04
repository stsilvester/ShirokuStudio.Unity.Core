using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;

namespace ShirokuStudio.Editor
{
    public class TagSelectorMenu
    {
        protected readonly string Splitter = ",";

        protected readonly IReactiveProperty<string> Property;
        protected readonly Func<IEnumerable<string>> TagOptionsGetter;
        protected IEnumerable<string> TagOptions;

        public TagSelectorMenu(IReactiveProperty<string> property, Func<IEnumerable<string>> tagOptionsGetter, string splitter = ",")
            : this(property, tagOptionsGetter(), splitter)
        {
            TagOptionsGetter = tagOptionsGetter;
        }

        public TagSelectorMenu(IReactiveProperty<string> property, IEnumerable<string> tagOptions, string splitter = ",")
        {
            Property = property;
            Splitter = splitter;
            TagOptions = tagOptions;
        }

        public void ShowAsContextMenu()
        {
            if (TagOptionsGetter != null)
                TagOptions = TagOptionsGetter();

            var nonSelectedOptions = TagOptions.Except(Property.Value.Split(Splitter));

            var menu = new GenericMenu();
            foreach (var option in nonSelectedOptions)
                menu.AddFuncItem(option, option, addItem);
            menu.ShowAsContext();
        }

        private void addItem(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return;

            var tagValue = Property.Value.Split(Splitter).Where(v => string.IsNullOrWhiteSpace(v) == false);
            if (tagValue.Contains(val))
                Property.Value = string.Join(Splitter, tagValue.Where(tag => tag != val));
            else
                Property.Value = string.Join(Splitter, tagValue.Concat(new[] { val }));
        }
    }
}