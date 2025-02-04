using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static class CustomGUIStyles
    {
        public static GUIStyle CloudTag => GUIStyleUtility.GetCachedGUIStyle("CloudTag",
            EditorStyles.miniButton,
            style =>
            {
                var tex = GUIStyleUtility.CreateBorderedTex(10, 10, 2, Color.grey, Color.green);

                style.fixedHeight = 0f;
                style.stretchHeight = true;
                style.padding = new RectOffset(4, 4, 2, 2);
                style.normal.textColor = Color.grey;

                style.onNormal.textColor = Color.green;
                style.active.textColor = Color.green;
                style.hover.textColor = Color.green;
                style.onHover.textColor = Color.green;
                style.focused.textColor = Color.green;
                style.onNormal.background = tex;
                style.onActive.background = tex;
            });


        public static GUIStyle TagWrap => GUIStyleUtility.GetCachedGUIStyle("TagWrap",
            EditorStyles.miniButton,
            style =>
            {
                style.padding = new RectOffset(16, 16, 2, 2);
            });

        public static GUIStyle TagLabel => GUIStyleUtility.GetCachedGUIStyle("TagLabel",
            EditorStyles.miniButtonLeft,
            style =>
            {
                var tex = style.normal.background;
                style.onNormal.background = tex;
                style.hover.background = tex;
                style.onHover.background = tex;
            });

        public static GUIStyle TagButton => GUIStyleUtility.GetCachedGUIStyle("TagButton",
            EditorStyles.miniButtonRight);

        public static GUIStyle Tab => GUIStyleUtility.GetCachedGUIStyle(nameof(Tab),
            EditorStyles.toolbarButton,
            style =>
            {
            });

        public static GUIStyle TabGroup => GUIStyleUtility.GetCachedGUIStyle(nameof(TabGroup),
            EditorStyles.toolbar,
            style => { });
    }
}