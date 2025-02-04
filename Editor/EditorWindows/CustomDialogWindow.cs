using System;
using UnityEditor;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public abstract class CustomDialogWindow<T> : EditorWindow
    {
        public T Data { get; set; }
        public string TextOK { get; set; } = "OK";
        public string TextCancel { get; set; } = "Cancel";
        public virtual Func<bool> CanOK { get; set; } = () => true;
        public virtual Func<bool> CanCancel { get; set; } = () => true;
        public virtual Action<T> OnOK { get; set; }
        public virtual Action<T> OnCancel { get; set; }

        private void OnGUI()
        {
            using var v = new EditorGUILayout.VerticalScope();
            onGUI();

            var isClose = false;
            using var h = new EditorGUILayout.HorizontalScope();
            {
                GUILayout.FlexibleSpace();
                using var d = new EditorGUI.DisabledGroupScope(!CanOK());
                if (GUILayout.Button(TextOK))
                {
                    HandleOK();
                    isClose = true;
                }
                d.Dispose();

                using var c = new EditorGUI.DisabledGroupScope(!CanCancel());
                if (GUILayout.Button(TextCancel))
                {
                    HandleCancel();
                    isClose = true;
                }
                c.Dispose();

                GUILayout.FlexibleSpace();
            }
            GUILayout.FlexibleSpace();
            v.Dispose();

            if (isClose)
                Close();
        }

        protected virtual void HandleOK()
        {
            OnOK?.Invoke(Data);
        }

        protected virtual void HandleCancel()
        {
            OnCancel?.Invoke(Data);
        }

        protected abstract void onGUI();
    }
}