using ShirokuStudio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public abstract partial class TreeViewEx<TNode> where TNode : TreeNodeEx
    {
        /// <summary>
        /// 搜尋列基底
        /// </summary>
        public abstract class SearchBase
        {
            private static Regex namePattern = new Regex(@"^(?<!:)\b[^\s:]+\b(?!:)");

            protected readonly TreeViewEx<TNode> tree;
            protected SearchEntryBase[] Entries { get; private set; }
            protected SearchField search;
            protected string searchName { get; private set; }
            protected bool hasSearchName;

            public SearchBase(TreeViewEx<TNode> tree)
            {
                this.tree = tree;
                search = new SearchField();
                Entries = SetupSearchParts().ToArray();
                Entries.Foreach(e => e.OnUpdated += UpdateSearchText);
            }

            protected abstract IEnumerable<SearchEntryBase> SetupSearchParts();

            public void HandleSearchChanged(string input)
            {
                var name = namePattern.Match(input);
                searchName = name.Success ? name.Value : "";
                hasSearchName = string.IsNullOrWhiteSpace(searchName) == false;
                foreach (var item in Entries)
                {
                    item.UpdateSearchInput(input);
                }
            }

            protected void UpdateSearchText()
            {
                tree.searchString = searchName + Entries.Where(e => e.IsActived).Join(" ");
            }

            public void DrawSearchBar()
            {
                DrawSearchBar(GUILayout.MinWidth(100), GUILayout.MaxWidth(200), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }

            public void DrawSearchBar(params GUILayoutOption[] options)
            {
                var searchRect = EditorGUILayout.GetControlRect(options);
                var popupRect = searchRect;
                popupRect.width = 20;
                if (Event.current.type == EventType.MouseDown && popupRect.Contains(Event.current.mousePosition))
                {
                    EditorGUIUtility.editingTextField = false;
                    showSeachOptions(popupRect);
                }
                else
                {
                    var newSearch = search.OnGUI(searchRect, tree.searchString);
                    if (newSearch != tree.searchString)
                    {
                        tree.searchString = newSearch;
                    }
                }
            }

            public void SetSearch(string prefix, string text)
            {
                var entry = Entries.OfType<SearchEntryBase>()
                    .FirstOrDefault(e => e.Prefix.Equals(prefix, StringComparison.CurrentCultureIgnoreCase));

                if (entry != null)
                {
                    entry.SearchText = text;
                    UpdateSearchText();
                }
            }

            public virtual bool IsMatch(TNode node)
            {
                return (hasSearchName == false || node.displayName?.IndexOf(searchName, StringComparison.OrdinalIgnoreCase) >= 0)
                    && Entries.Where(e => e.IsActived).All(e => e.Match(node));
            }

            protected virtual void showSeachOptions(Rect rect)
            {
                var menu = new GenericMenu();
                menu.AddFuncItem("清空", () => tree.searchString = ""
                    , enabled: string.IsNullOrEmpty(tree.searchString) == false);
                foreach (var optionHandler in Entries.OfType<SearchEntryBase>().OfType<ISearchOption>())
                {
                    if (optionHandler.Options?.Any() != true)
                        continue;

                    var entry = optionHandler as SearchEntryBase;

                    var category = string.IsNullOrWhiteSpace(optionHandler.OptionCategory) 
                        ? "" : $"{optionHandler.OptionCategory}/";

                    foreach (var opt in optionHandler.Options)
                    {
                        var isOn = entry.IsActived && opt.IsOn();
                        var name = opt.Name;
                        menu.AddFuncItem(category + name, () => opt.OnToggle(), isOn: isOn);
                    }
                }
                appendSearchMenu(menu);
                menu.DropDown(rect);
            }

            protected virtual void appendSearchMenu(GenericMenu menu)
            {
                menu.AddSeparator();
            }
        }

        private struct EntryText
        {
            public SearchEntry Entry;
            public string Preset;

            public EntryText(SearchEntry entry, string preset)
            {
                Entry = entry;
                Preset = preset;
            }
        }

        public struct OptionMenuItem
        {
            public string Name { get; set; }
            public Action OnToggle { get; set; }
            public Func<bool> IsOn { get; set; }
        }
        
        public interface ISearchOption
        {
            string OptionCategory { get; }
            IEnumerable<OptionMenuItem> Options { get; }
        }

        public class SearchEntry : SearchEntryBase, ISearchOption
        {
            public override bool IsActived => isActived;
            public readonly Func<TNode, string> PropertyGetter;

            private bool isActived { get; set; }

            public override string SearchText
            {
                get => matchText;
                set
                {
                    matchText = value;
                    isActived = string.IsNullOrWhiteSpace(matchText) == false;
                    isRegex = isActived && matchText.Contains("*");
                    RaiseUpdated();
                }
            }

            private bool isRegex { get; set; }
            private string matchText;
            public string OptionCategory { get; private set; }
            public IEnumerable<OptionMenuItem> Options => getOptions();

            private IEnumerable<OptionMenuItem> getOptions()
            {
                if (options.Any())
                    foreach (var item in options)
                        yield return item;

                if (optionGetter != null)
                    foreach (var item in optionGetter.Invoke())
                        yield return item;
            }

            private List<OptionMenuItem> options = new();
            private Func<IEnumerable<OptionMenuItem>> optionGetter;

            public SearchEntry(string prefix, Func<TNode, string> getter)
                : base(prefix, @"(\w+)")
            {
                PropertyGetter = getter;
            }

            public SearchEntry AddOptions(string cat, params (string name, string value)[] options)
            {
                OptionCategory = cat;
                this.options.AddRange(options.Select(opt => new OptionMenuItem
                {
                    Name = opt.name,
                    IsOn = () => ToggleOptionIsOn(opt.value),
                    OnToggle = () => ToggleOption(opt.value)
                }));
                return this;
            }

            public SearchEntry AddOptions(string cat, Func<(string name, string value)[]> optionGetter)
            {
                OptionCategory = cat;
                this.optionGetter = () => optionGetter().Select(opt => new OptionMenuItem
                {
                    Name = opt.name,
                    IsOn = () => ToggleOptionIsOn(opt.value),
                    OnToggle = () => ToggleOption(opt.value)
                });
                return this;
            }

            public override void UpdateSearchInput(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    SearchText = "";
                    return;
                }
                var m = Pattern.Match(input);
                if (m.Success)
                {
                    SearchText = m.Groups[1].Value;
                }
                else
                {
                    SearchText = "";
                }
            }

            public override bool Match(TNode node)
            {
                if (isActived == false)
                    return true;

                var target = PropertyGetter(node);
                if (string.IsNullOrWhiteSpace(target))
                    return false;

                return target.Contains(SearchText);
            }

            protected bool ToggleOptionIsOn(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }
                return SearchText.Equals(value, StringComparison.OrdinalIgnoreCase);
            }

            protected void ToggleOption(string value)
            {
                SearchText = (SearchText ?? "").Equals(value, StringComparison.OrdinalIgnoreCase) ? "" : value;
            }
        }

        protected class FlagSearchEntry<TEnumFlag> : SearchEntryBase, ISearchOption
            where TEnumFlag : Enum
        {
            public string OptionCategory { get; }
            public readonly Func<TNode, TEnumFlag> PropertyGetter;

            public override string SearchText
            {
                get => cachedSearchText;
                set => SearchValue = EnumUtility.ParseEnumFlag<TEnumFlag>(value);
            }
            private string cachedSearchText;

            public IEnumerable<OptionMenuItem> Options { get; }

            public TEnumFlag SearchValue
            {
                get => searchFlag;
                set
                {
                    searchFlag = value;
                    cachedSearchText = searchFlag.GetDisplayName();
                    RaiseUpdated();
                }
            }
            private TEnumFlag searchFlag;
            public override bool IsActived => !SearchValue.Equals( default(TEnumFlag));
            

            public FlagSearchEntry(string prefix, string category, Func<TNode, TEnumFlag> propertyGetter)
                : base(prefix, @"([\S,]+)")
            {
                PropertyGetter = propertyGetter;
                OptionCategory = category;
                Options = EnumUtility.GetFlags<TEnumFlag>()
                    .Select(flag => new OptionMenuItem
                    {
                        Name = flag.GetDisplayName(),
                        IsOn = () => SearchValue.HasFlag(flag),
                        OnToggle = () => SearchValue = SearchValue.Toggle(flag)
                    }).ToArray();
            }

            public override void UpdateSearchInput(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    SearchValue = (TEnumFlag)(object)0;
                    return;
                }
                var m = Pattern.Match(input);
                if (m.Success)
                {
                    SearchText = m.Groups[1].Value;
                }
                else
                {
                    SearchText = "";
                }
            }

            public override bool Match(TNode node)
            {
                var value = PropertyGetter(node);
                return value.HasFlag(SearchValue);
            }
        }

        protected class BasicSearch : SearchBase
        {
            public BasicSearch(TreeViewEx<TNode> tree) : base(tree)
            {
            }

            protected override IEnumerable<SearchEntryBase> SetupSearchParts()
            {
                yield break;
            }
        }
    }
}