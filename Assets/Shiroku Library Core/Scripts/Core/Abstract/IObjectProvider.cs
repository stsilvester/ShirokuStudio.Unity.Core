namespace ShirokuStudio.Core
{
    public interface IObjectProvider
    {
        /// <summary>
        /// 以ID取得Unity物件
        /// </summary>
        T GetObject<T>(string id) where T : UnityEngine.Object;
    }
}