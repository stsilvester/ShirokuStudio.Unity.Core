using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace UnityEngine.AddressableAssets
{
    [Flags]
    public enum AutoHideTypes
    {
        HideWhenLoading = 1,
        HideWhenNoSprite = 2,
        ForceShowWhenLoaded = 4,

        Default = HideWhenNoSprite
    }

    public static class AddressableExtensions
    {
        /// <summary>
        /// 同步方式取得或加載資源
        /// </summary>
        public static T GetOrLoadAsset<T>(this AssetReferenceT<T> addr)
            where T : Object
        {
            if (addr == null)
                return default(T);

            if (addr.IsValid() == false)
                addr.LoadAssetAsync().WaitForCompletion();

            return addr.Asset as T;
        }

        /// <summary>
        /// 非同步方式取得或加載資源
        /// </summary>
        public static async Task<T> GetOrLoadAssetAsync<T>(this AssetReferenceT<T> addr)
            where T : Object
        {
            if (addr == null)
                return default(T);

            if (addr.RuntimeKeyIsValid() == false)
                return default(T);

            if (addr.IsValid() == false)
                await addr.LoadAssetAsync();

            if (addr.IsDone == false)
                await addr.OperationHandle;

            if (addr.Asset is T t)
                return t;

            if (addr.Asset is GameObject go && typeof(T).IsAssignableFrom(typeof(Component)))
                return go.GetComponent<T>();

            if (addr.Asset is SpriteAtlas atlas)
                return atlas.GetSprite(addr.SubObjectName) as T;

            return default(T);
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public static void ReleaseAssets(this IEnumerable<AssetReference> addrs)
        {
            foreach (var item in addrs.Where(addr => addr.IsValid()))
                item.ReleaseAsset();
        }

        public static async UniTask TrySetSprite(this Image img, AssetReferenceSprite addr, AutoHideTypes autoHide = AutoHideTypes.Default)
        {
            if (img == null || addr == null)
                return;

            if (addr.RuntimeKeyIsValid() == false)
                return;

            if (autoHide.HasFlag(AutoHideTypes.HideWhenLoading))
                img.enabled = false;
            else if (autoHide.HasFlag(AutoHideTypes.HideWhenNoSprite) && img.sprite == false)
                img.enabled = false;

            var sprite = await addr.GetOrLoadAssetAsync();
            if (sprite)
                img.sprite = sprite;

            img.enabled = autoHide.HasFlag(AutoHideTypes.ForceShowWhenLoaded) ||
                (autoHide.HasFlag(AutoHideTypes.HideWhenNoSprite) ? img.sprite : true);
        }
    }
}

namespace Common
{
}