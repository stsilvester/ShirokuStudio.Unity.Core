using System;
using UnityEditor;

namespace ShirokuStudio.Editor
{
    public static class DialogUtility
    {
        public static T GetWindow<T, D>(D data, Action<D> onOK = null, Action<D> onCancel = null, string title = null, string ok = null, string cancel = null)
            where T : CustomDialogWindow<D>
        {
            var window = EditorWindow.GetWindow<T>(title);
            window.Data = data ?? window.Data;
            window.TextOK = ok ?? window.TextOK;
            window.TextCancel = cancel ?? window.TextCancel;
            window.OnOK = onOK ?? window.OnOK;
            window.OnCancel = onCancel ?? window.OnCancel;
            return window;
        }
    }
}
