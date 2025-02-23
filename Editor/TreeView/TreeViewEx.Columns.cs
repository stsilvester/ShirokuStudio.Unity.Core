using System;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public abstract partial class TreeViewEx<TNode> where TNode : TreeNodeEx
    {
        protected partial class Column
        {
            public class Label : Column
            {
                public Label(TreeViewEx<TNode> tree, string header, Func<TNode, object> getter) : base(tree, header)
                {
                    UseDefault = false;
                    displayNameGetter = t => getter?.Invoke(t)?.ToString();
                }
            }

            public class Icon : Column
            {
                public Func<TNode, Texture> TextureGetter;

                public Icon(TreeViewEx<TNode> tree, string header, Func<TNode, Texture> textureGetter) : this(tree, textureGetter, header)
                {
                }

                public Icon(TreeViewEx<TNode> tree, Func<TNode, Texture> textureGetter, string header = null) : base(tree, header)
                {
                    TextureGetter = textureGetter;
                    UseDefault = false;
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode item, string displayName)
                {
                    var texture = TextureGetter(item);
                    if (texture)
                    {
                        GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
                    }
                }
            }

            #region 可輸入欄位

            public class SerializedValue : Column
            {
                public Func<TNode, SerializedProperty> GetProperty;
                public Action<TNode> OnChanged;

                public SerializedValue(TreeViewEx<TNode> tree, string header, Func<TNode, SerializedProperty> getProp)
                    : base(tree, header)
                {
                    UseDefault = false;
                    GetProperty = getProp;
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode item, string displayName)
                {
                    var lbw = EditorGUIUtility.labelWidth;
                    EditorGUI.BeginChangeCheck();
                    EditorGUIUtility.labelWidth = 1;
                    EditorGUI.PropertyField(rect, GetProperty(item), new GUIContent(displayName), true);
                    EditorGUIUtility.labelWidth = lbw;
                    if (EditorGUI.EndChangeCheck())
                    {
                        OnChanged?.Invoke(item);
                        GUI.changed = true;
                    }
                }
            }

            public class Property : SerializedValue
            {
                public readonly Type DrawerType;
                private PropertyDrawer drawer;

                public Property(TreeViewEx<TNode> tree, string header, Type drawerType, Func<TNode, SerializedProperty> getProp)
                    : base(tree, header, getProp)
                {
                    DrawerType = drawerType;
                    drawer = (PropertyDrawer)Activator.CreateInstance(DrawerType);
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode item, string displayName)
                {
                    drawer.OnGUI(rect, GetProperty(item), new GUIContent(displayName));
                }
            }

            public class Property<TDrawer> : SerializedValue where TDrawer : PropertyDrawer
            {
                private TDrawer drawer;

                public Property(TreeViewEx<TNode> tree, string header, Func<TNode, SerializedProperty> getProp)
                    : base(tree, header, getProp)
                {
                    drawer = Activator.CreateInstance<TDrawer>();
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode item, string displayName)
                {
                    drawer.OnGUI(rect, GetProperty(item), GUIContent.none);
                }
            }

            #region 單一值輸入欄位

            public abstract class Input<T> : Column
            {
                public Func<TNode, T> GetInputValue;
                public Action<TNode, T> HandleValueChanged;
                public bool displayNameFallback = false;

                protected Input(TreeViewEx<TNode> tree, string header, Func<TNode, T> getValue, Action<TNode, T> handle)
                    : base(tree, header)
                {
                    UseDefault = false;
                    GetInputValue = getValue;
                    HandleValueChanged = handle;
                }

                protected Input(TreeViewEx<TNode> tree, Func<TNode, T> getValue, Action<TNode, T> handle)
                    : this(tree, null, getValue, handle)
                {
                }
            }

            public class TextInput : Input<string>
            {
                private bool? isRevert = null;

                public TextInput(TreeViewEx<TNode> tree, string header, Func<TNode, string> getValue, Action<TNode, string> handle)
                    : base(tree, header, getValue, handle)
                {
                }

                public TextInput(TreeViewEx<TNode> tree, Func<TNode, string> getValue, Action<TNode, string> handle)
                    : this(tree, null, getValue, handle)
                {
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    if (node.IsEditing)
                    {
                        EditorGUI.BeginChangeCheck();
                        if (IsHierarchy)
                        {
                            rect.xMin += Tree.GetContentIndent(args.item);
                        }
                        if (ColumnIndex == 0)
                        {
                            var rect_edit = rect;
                            rect_edit.width = 8f;
                            EditorGUI.LabelField(rect_edit, "*");
                            rect.xMin += 8f;
                        }
                        if (node is IRevertable revertable)
                        {
                            var btnWidth = 20f;
                            var rect_apply = rect;
                            rect_apply.width = btnWidth;
                            rect.xMin += btnWidth;
                            if (GUI.Button(rect_apply, "✓"))
                            {
                                isRevert = false;
                                node.IsEditing = false;
                                revertable.Apply();
                            }
                            var rect_revert = rect;
                            rect_revert.width = btnWidth;
                            rect.xMin += btnWidth;
                            if (GUI.Button(rect_revert, "X"))
                            {
                                isRevert = true;
                                node.IsEditing = false;
                                revertable.Revert();
                            }
                        }

                        var value = EditorGUI.TextField(rect, GetInputValue(node));
                        if (EditorGUI.EndChangeCheck())
                        {
                            node.Changed = true;
                            Tree.stateEx.isChanged = true;
                            HandleValueChanged(node, value);
                        }
                    }
                    else
                    {
                        if (displayNameFallback)
                        {
                            defaultGUI(rect, ref args);
                        }
                        else
                        {
                            base.onGUI(rect, ref args, node, GetInputValue.Invoke(node));
                        }
                    }
                }
            }

            public class IntInput : Input<int>
            {
                public string Format = "";

                public IntInput(TreeViewEx<TNode> tree, string header, Func<TNode, int> getValue, Action<TNode, int> handle)
                    : base(tree, header, getValue, handle)
                {
                }

                public IntInput(TreeViewEx<TNode> tree, Func<TNode, int> getValue, Action<TNode, int> handle)
                    : this(tree, null, getValue, handle)
                {
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    if (node.IsEditing)
                    {
                        EditorGUI.BeginChangeCheck();
                        var value = EditorGUI.IntField(rect, GetInputValue(node));
                        if (EditorGUI.EndChangeCheck())
                        {
                            node.Changed = true;
                            Tree.stateEx.isChanged = true;
                            HandleValueChanged(node, value);
                        }
                    }
                    else
                    {
                        base.onGUI(rect, ref args, node, GetInputValue.Invoke(node).ToString(Format));
                    }
                }
            }

            public class IntSlider : IntInput
            {
                public int Min;
                public int Max;

                public IntSlider(TreeViewEx<TNode> tree, Func<TNode, int> getValue, Action<TNode, int> handle) : base(tree, getValue, handle)
                {
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    EditorGUI.BeginChangeCheck();
                    var value = EditorGUI.IntSlider(rect, GetInputValue(node), Min, Max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        node.Changed = true;
                        Tree.stateEx.isChanged = true;
                        HandleValueChanged(node, value);
                    }
                }
            }

            public class FloatInput : Input<float>
            {
                public string Format = "";

                public FloatInput(TreeViewEx<TNode> tree, string header, Func<TNode, float> getValue, Action<TNode, float> handle)
                    : base(tree, header, getValue, handle)
                {
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    if (node.IsEditing)
                    {
                        EditorGUI.BeginChangeCheck();
                        var value = EditorGUI.FloatField(rect, GetInputValue(node));
                        if (EditorGUI.EndChangeCheck())
                        {
                            node.Changed = true;
                            Tree.stateEx.isChanged = true;
                            HandleValueChanged(node, value);
                        }
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, GetInputValue.Invoke(args.item as TNode).ToString(Format));
                    }
                }
            }

            public class FloatSlider : FloatInput
            {
                public float Min;
                public float Max;

                public FloatSlider(TreeViewEx<TNode> tree, string header, Func<TNode, float> getValue, Action<TNode, float> handle)
                    : base(tree, header, getValue, handle)
                {
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    EditorGUI.BeginChangeCheck();
                    var value = EditorGUI.Slider(rect, GetInputValue(node), Min, Max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        node.Changed = true;
                        Tree.stateEx.isChanged = true;
                        HandleValueChanged(node, value);
                    }
                }
            }

            #endregion 單一值輸入欄位

            public abstract class Input<T, U> : Column
            {
                public Func<TNode, T> GetInputValue_1;
                public Func<TNode, T> GetInputValue_2;
                public Action<TNode, T, U> HandleValueChanged;
                public bool displayNameFallback = false;

                protected Input(TreeViewEx<TNode> tree, string header
                    , Func<TNode, T> getValue, Func<TNode, T> getValue2
                    , Action<TNode, T, U> handle2)
                    : base(tree, header)
                {
                    UseDefault = false;
                    GetInputValue_1 = getValue;
                    GetInputValue_2 = getValue2;
                    HandleValueChanged = handle2;
                }
            }

            public class IntRange : Input<int, int>
            {
                public string Separator = "-";
                public float SeparatorWidth = EditorGUIUtility.standardVerticalSpacing;

                public IntRange(TreeViewEx<TNode> tree, string header
                    , Func<TNode, int> getValue, Func<TNode, int> getValue2
                    , Action<TNode, int, int> handle)
                    : base(tree, header, getValue, getValue2, handle)
                {
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    if (node.IsEditing)
                    {
                        var rect1 = rect;
                        rect1.width = rect.width / 2 - SeparatorWidth / 2;
                        var rect2 = rect;
                        rect2.xMin = rect1.xMax + SeparatorWidth;
                        var rectInter = rect;
                        rectInter.xMin = rect1.xMax;
                        rectInter.xMax = rect2.xMin;

                        var preVal1 = GetInputValue_1(node);
                        var value1 = EditorGUI.IntField(rect1, preVal1);
                        EditorGUI.LabelField(rectInter, Separator);
                        var preVal2 = GetInputValue_2(node);
                        var value2 = EditorGUI.IntField(rect2, preVal2);

                        if (value1 != preVal1 || preVal2 != value2)
                        {
                            node.Changed = true;
                            Tree.stateEx.isChanged = true;
                            HandleValueChanged.Invoke(node, value1, value2);
                        }
                    }
                    else
                    {
                        var val1 = GetInputValue_1(node);
                        var val2 = GetInputValue_2(node);
                        base.onGUI(rect, ref args, node, $"{val1}{Separator}{val2}");
                    }
                }
            }

            public class Dropdown<TObject> : Input<TObject>
            {
                public (string, TObject)[] Presets;

                public Dropdown(TreeViewEx<TNode> tree, string header, Func<TNode, TObject> getValue, Action<TNode, TObject> handle,
                    params (string, TObject)[] presets)
                    : base(tree, header, getValue, handle)
                {
                    Presets = presets;
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    if (node.IsEditing)
                    {
                        var val = GetInputValue(node);
                        var selection = Presets.FirstOrDefault(s => s.Item2.Equals(val));
                        if (EditorGUI.DropdownButton(rect, new GUIContent(selection.Item1 ?? "未指定", tooltipGetter?.Invoke(node)), FocusType.Passive))
                        {
                            var menu = new GenericMenu();
                            foreach (var item in Presets)
                            {
                                menu.AddFuncItem(item.Item1, item.Item2, v => HandleValueChanged(node, v));
                            }
                            menu.DropDown(rect);
                        }
                    }
                    else
                    {
                        base.onGUI(rect, ref args, node, displayName);
                    }
                }
            }

            public class Object<TObject> : Input<TObject> where TObject : UnityEngine.Object
            {
                public Object(TreeViewEx<TNode> tree, string header, Func<TNode, TObject> getValue, Action<TNode, TObject> handle)
                    : base(tree, header, getValue, handle)
                {
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    EditorGUI.BeginChangeCheck();
                    var value = (TObject)EditorGUI.ObjectField(rect, GetInputValue(node), typeof(TObject), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        node.Changed = true;
                        Tree.stateEx.isChanged = true;
                        HandleValueChanged(node, value);
                    }
                }
            }

            public class FloatMinMax : Input<float, float>
            {
                private readonly Func<TNode, (float, float)> borders;
                public int Digit = 4;
                private static float[] portions = new[] { .5f, .49f / 2, .01f, .49f / 2 };

                public FloatMinMax(TreeViewEx<TNode> tree, string header
                    , Func<TNode, float> getValue
                    , Func<TNode, float> getValue2
                    , Action<TNode, float, float> handle2
                    , Func<TNode, (float, float)> getBorders)
                    : base(tree, header, getValue, getValue2, handle2)
                {
                    borders = getBorders;
                    minWidth = 200;
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode node, string displayName)
                {
                    if (node.IsEditing)
                    {
                        var r = rect;
                        var preVal1 = GetInputValue_1(node);
                        var preVal2 = GetInputValue_2(node);
                        var value1 = preVal1;
                        var value2 = preVal2;
                        var limits = borders(node);
                        var rect_Slider = r;
                        rect_Slider.width = r.width * portions[0];
                        EditorGUI.MinMaxSlider(rect_Slider, ref value1, ref value2, limits.Item1, limits.Item2);

                        var rect_min = rect_Slider;
                        rect_min.xMin = rect_Slider.xMax;
                        rect_min.width = r.width * portions[1];
                        value1 = Mathf.Clamp(EditorGUI.FloatField(rect_min, value1), limits.Item1, limits.Item2);

                        var rect_max = rect_min;
                        rect_max.xMin = rect_min.xMax + r.width * portions[2];
                        rect_max.width = r.width * portions[3];
                        value2 = Mathf.Clamp(EditorGUI.FloatField(rect_max, value2), limits.Item1, limits.Item2);
                        if (value1 != preVal1 || preVal2 != value2)
                        {
                            node.Changed = true;
                            Tree.stateEx.isChanged = true;
                            HandleValueChanged.Invoke(node, value1, value2);
                        }
                    }
                    else
                    {
                        var val1 = GetInputValue_1(node);
                        var val2 = GetInputValue_2(node);
                        base.onGUI(rect, ref args, node, $"{Math.Round(val1, Digit)},{Math.Round(val2, Digit)}");
                    }
                }
            }

            #endregion 可輸入欄位

            public class Button : Column
            {
                public Action<TNode> OnClick;
                public Func<TNode, bool> disable;

                public Button(TreeViewEx<TNode> tree, string header, Action<TNode> onclick) : base(tree, header)
                {
                    OnClick = onclick;
                }

                public Button(TreeViewEx<TNode> tree, Action<TNode> onclick) : base(tree)
                {
                    OnClick = onclick;
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode item, string displayName)
                {
                    var text = new GUIContent(displayName, tooltipGetter?.Invoke(item));
                    using (new EditorGUI.DisabledGroupScope(disable?.Invoke(item) == true))
                    {
                        rect.height = EditorGUIUtility.singleLineHeight;
                        if (GUI.Button(rect, text))
                        {
                            OnClick?.Invoke(item);
                        }
                    }
                }
            }

            public class Any : Column
            {
                public struct Parameter
                {
                    public Rect rect;
                    public RowGUIArgs args;
                    public TNode item;
                    public string displayName;
                }

                public Action<Parameter> OnGUIDelegate;

                public Any(TreeViewEx<TNode> tree, string header, Action<Parameter> ongui) : base(tree, header)
                {
                    OnGUIDelegate = ongui;
                    UseDefault = false;
                }

                protected override void onGUI(Rect rect, ref RowGUIArgs args, TNode item, string displayName)
                {
                    if (OnGUIDelegate != null)
                    {
                        OnGUIDelegate(new Parameter
                        {
                            rect = rect,
                            args = args,
                            item = item,
                            displayName = displayName
                        });
                    }
                    else
                    {
                        base.onGUI(rect, ref args, item, displayName);
                    }
                }
            }
        }
    }
}