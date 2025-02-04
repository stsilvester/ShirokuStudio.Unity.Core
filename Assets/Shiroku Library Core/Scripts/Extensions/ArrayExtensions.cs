namespace System.Linq
{
    /// <summary>
    /// 陣列擴充方法
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// 取得第一個符合條件的元素的索引值
        /// </summary>
        /// <typeparam name="T">任意類型</typeparam>
        /// <param name="src">來源陣列</param>
        /// <param name="checker">檢測條件</param>
        /// <returns>符合條件的元素的索引</returns>
        public static int IndexOf<T>(this T[] src, Func<T, bool> checker)
        {
            if (checker == null)
            {
                throw new ArgumentNullException("checker");
            }
            for (var i = 0; i < src.Length; i++)
            {
                if (checker(src[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 取得第一個符合條件的元素的索引值
        /// </summary>
        /// <typeparam name="T">任意類型</typeparam>
        /// <param name="src">來源陣列</param>
        /// <param name="checker">檢測條件</param>
        /// <returns>符合條件的元素的索引</returns>
        public static int IndexOf<T>(this T[] src, T checker) where T : class
        {
            if (checker == null)
            {
                throw new ArgumentNullException("checker");
            }
            for (var i = 0; i < src.Length; i++)
            {
                if (checker.Equals(src[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool TryGetElementAt<T>(this T[] src, int index, out T value)
        {
            if (src?.Any() != true
                || index > src.Length
                || index < 0)
            {
                value = default;
                return false;
            }
            value = src[index];
            return true;
        }
    }
}