namespace System
{
    public static class BooleanExtensions
    {
        /// <summary>
        /// 根據bool值執行對應的Action
        /// </summary>
        /// <param name="onEnable">當值為True的觸發動作</param>
        /// <param name="onDisable">當值為False的觸發動作</param>
        public static void Toggle(this bool value, Action onEnable, Action onDisable)
        {
            if (value)
                onEnable?.Invoke();
            else
                onDisable?.Invoke();
        }
    }
}
