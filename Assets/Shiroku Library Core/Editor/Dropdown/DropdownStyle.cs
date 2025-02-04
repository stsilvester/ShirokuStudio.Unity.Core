using ShirokuStudio.Core;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    internal static class DropdownStyle
    {
        public const float NodeHeight = 23f;
        public const float IndentWidth = 14f;
        public const float IconSize = 14f;
        public const float GlobalOffset = 14f;
        public const int WindowMaxHeight = 600;

        public static Color SelectedColor = new Color(0.24f, 0.48f, 0.9f, 1f);
        public static Color HighlightedColor = new Color(0.24f, 0.48f, 0.9f, 0.5f);
        public static Color DarkSeparatorLine = new Color(0.2f, 0.2f, 0.2f, 1f);
        public static Color LightSeparatorLine = new Color(0.4f, 0.4f, 0.4f, 1f);
        public const float LabelHeight = 32f;

        public static LazyValue<GUIStyle> DefaultLabelStyle => new(() => new GUIStyle(EditorStyles.label)
        {
        });

        public static LazyValue<GUIStyle> SelectedLabelStyle => new(() => new GUIStyle(EditorStyles.label)
        {
        });

        public static LazyValue<GUIStyle> NoPadding => new(() => new GUIStyle()
        {
            padding = new RectOffset()
        });

        public static LazyValue<GUIStyle> Box => new(()
            => GUIStyleUtility.GetCachedGUIStyle("DropdownStyles.Box", GUI.skin.box, style =>
            {
                var background = GUIStyleUtility.CreateBorderedTex(1,
                    new Color(0.8f, 0.3f, 0.6f, 1f),
                    Color.black);
                style.border = new RectOffset(2, 2, 2, 2);
                style.padding = new RectOffset(2, 2, 2, 2);
                style.normal.background = background;
            }));
    }
}