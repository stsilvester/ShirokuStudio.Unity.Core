using UnityEngine;

namespace ShirokuStudio.Core
{
    public static class PlayerPrefsUtility
    {
        public static bool TryGetString(string key, out string value)
        {
            if (PlayerPrefs.HasKey(key))
            {
                value = PlayerPrefs.GetString(key);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public static bool TryGet<T>(string key, out T value)
        {
            if (PlayerPrefs.HasKey(key))
            {
                var raw = PlayerPrefs.GetString(key);
                value = JsonUtility.FromJson<T>(raw);
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public static T Get<T>(string key)
        {
            var raw = PlayerPrefs.GetString(key);
            if (raw == null)
            {
                return default(T);
            }

            return JsonUtility.FromJson<T>(raw);
        }
    }
}