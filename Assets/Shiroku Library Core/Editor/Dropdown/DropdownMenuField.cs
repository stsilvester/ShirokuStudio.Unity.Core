using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShirokuStudio.Editor
{
    public class DropdownMenuField<TOption> : BaseField<TOption>
    {
        public new class UxmlFactory : UxmlFactory<DropdownMenuField<TOption>, UxmlTraits>
        { }

        public new class UxmlTraits : BaseField<string>.UxmlTraits
        { }

        protected DropdownMenu<TOption> DropdownMenu
            => dropdownMenu ??= new DropdownMenu<TOption>(dropdownItems, updateValue);

        private DropdownMenu<TOption> dropdownMenu;

        public virtual List<DropdownItem<TOption>> Choices
        {
            get => dropdownItems;
            set
            {
                dropdownItems = value;
                dropdownMenu = null;
            }
        }

        protected List<DropdownItem<TOption>> dropdownItems = new();

        public Func<TOption, string> GetDisplatText = opt => opt?.ToString() ?? "null";

        private Button button;

        public DropdownMenuField() : this("", null)
        {
        }

        public DropdownMenuField(string label) : this(label, null)
        {
        }

        public DropdownMenuField(string label, VisualElement visualInput) : base(label, visualInput)
        {
            button = new Button(() => DropdownMenu.ShowAsContext(300));
            button.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
            Add(button);

            style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
            style.alignItems = Align.FlexEnd;
        }

        private void updateValue(TOption option)
        {
            value = option;
            SetValueWithoutNotify(option);
        }

        public override void SetValueWithoutNotify(TOption newValue)
        {
            base.SetValueWithoutNotify(newValue);
            button.text = GetDisplatText(newValue);
        }
    }
}