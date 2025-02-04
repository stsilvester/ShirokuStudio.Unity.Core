using UnityEditor;

namespace ShirokuStudio.Editor
{
    public class FoldoutScope : GUIScope
    {
        public bool Foldout { get; }

        public FoldoutScope(bool foldout) : base(EditorGUILayout.EndFoldoutHeaderGroup)
        {
            Foldout = foldout;
        }
    }
}