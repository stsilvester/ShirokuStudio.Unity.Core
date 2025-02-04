using System;
using UnityEditor;

namespace ShirokuStudio.Editor
{
    public interface IDataEditor<T>
    {
        T Data { get; }

        event Action<T> OnApply;

        event Action<T> OnDiscard;

        void DrawEditor();
    }
}

namespace ShirokuStudio.Editor
{
    public abstract class DataEditor<T> : IDataEditor<T>
    {
        public event Action<T> OnApply;

        public event Action<T> OnDiscard;

        protected IDataEditorView<T> CurrentView;

        public T Data { get; private set; }

        public SerializedObject SerializedObject { get; private set; }

        public SerializedProperty this[string propertyName] => SerializedObject?.FindProperty(propertyName);

        public virtual void SetTarget(T data)
        {
            if (Equals(Data, data))
                return;

            Data = data;
            SerializedObject = data is UnityEngine.Object uo
                ? new SerializedObject(uo)
                : null;

            if (Data is UnityEngine.Object u && u)
                Undo.RegisterCompleteObjectUndo(u, "Edit " + u.name);

            CurrentView?.OnSetTarget(data);
        }

        public abstract void DrawEditor();

        public virtual void Apply()
        {
            if (SerializedObject != null)
                SerializedObject.ApplyModifiedProperties();

            if (Data is UnityEngine.Object u && u)
            {
                Undo.FlushUndoRecordObjects();
                Undo.RegisterCompleteObjectUndo(u, "Edit " + u.name);
                AssetDatabase.SaveAssetIfDirty(u);
            }

            OnApply?.Invoke(Data);
        }

        public virtual void Discard()
        {
            if (Data is UnityEngine.Object u)
                Undo.ClearUndo(u);

            SerializedObject?.Update();
            OnDiscard?.Invoke(Data);
        }

        public void RecordUndo(string name, Action action, UnityEngine.Object target = null)
        {
            if (target == null)
                target = Data as UnityEngine.Object;

            if (target != null)
                Undo.RecordObject(target, name);

            action();
        }
    }

    public interface IDataEditorView<T>
    {
        void OnSetTarget(T target);
        void OnGUI_Top();
        void OnGUI_Content();
    }
}

namespace ShirokuStudio.Editor
{
    public interface IDataFieldDrawer<T>
    {
        T OnGUI(string label, T data);
    }
}