using System;
using ShirokuStudio.Editor;
using UnityEngine;
using Zenject;

namespace UnityEditor
{
    public abstract class SelectorWindow<TData> : SimpleZenjectEditorWindow
    {
        /// <summary>於<see cref="Complete(TData)"/>後仍保持視窗開啟</summary>
        public virtual bool EnableKeepAlive { get; set; }

        /// <summary>當視窗失焦時自動關閉視窗</summary>
        public virtual bool EnableCloseOnBlur { get; set; } = true;

        public Action<TData> Callback { get; set; }

        public event Action OnClose;

        protected bool close { get; set; }

        private bool initialized;

        public virtual void Complete(TData data)
        {
            Callback?.Invoke(data);
            if (EnableKeepAlive == false)
            {
                close = true;
            }
        }

        public override void InstallBindings() { }

        protected override void Initialize()
        {
            base.Initialize();
            initialized = true;
        }

        protected virtual void OnLostFocus()
        {
            if (EnableCloseOnBlur)
            {
                close = true;
            }
        }

        override public void OnGUI()
        {
            if (!initialized)
                return;

            if (close)
            {
                OnClose?.Invoke();
                Close();
                GUIUtility.ExitGUI();
                return;
            }
            DrawContent();
        }

        protected abstract void DrawContent();

        public static TSelector GetSelector<TSelector>(string windowName, Action<TData> callback = null, bool keepAlive = false) where TSelector : SelectorWindow<TData>
        {
            var selector = GetWindow<TSelector>(true, windowName);
            selector.Callback = callback;
            selector.EnableKeepAlive = keepAlive;
            return selector;
        }

        protected static void drawField(string label, int id
            , Func<int, TData> getTarget
            , Func<TData, string> displayName
            , Action<Action<TData>> selector
            , Action<TData> callback)
        {
            var item = getTarget(id);
            drawField(label, item, displayName, selector, callback);
        }

        protected static void drawField(string label, string id
            , Func<string, TData> getTarget
            , Func<TData, string> displayName
            , Action<Action<TData>> selector
            , Action<TData> callback)
        {
            var item = getTarget(id);
            drawField(label, item, displayName, selector, callback);
        }

        protected static void drawField(string label, TData item
            , Func<TData, string> displayName
            , Action<Action<TData>> selector
            , Action<TData> callback)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (string.IsNullOrWhiteSpace(label) == false)
                {
                    EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15f));
                }
                if (GUILayout.Button(displayName(item) ?? "(無)"))
                {
                    selector(callback);
                }
                GUILayout.FlexibleSpace();
            }
        }
    }
}