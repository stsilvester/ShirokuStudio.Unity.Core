using System.Security.Cryptography;
using UnityEngine;

namespace ShirokuStudio.Core
{
    public static class Utility
    {
        public static float Random(float min, float max)
        {
            using (var random = RandomNumberGenerator.Create())
            {
                var buffer = new byte[32];
                random.GetBytes(buffer);
                var randUint = System.BitConverter.ToUInt32(buffer, 0);
                var normalize = randUint / (uint.MaxValue + 1f);
                return min + normalize * (max - min);
            }
        }

        public static int Random(int min, int max)
        {
            using (var random = RandomNumberGenerator.Create())
            {
                var buffer = new byte[32];
                random.GetBytes(buffer);
                var randUint = System.BitConverter.ToUInt32(buffer, 0);
                return min + (int)(randUint % (max - min));
            }
        }

        public static Vector2 Clone(Vector2 target, float? x = null, float? y = null)
        {
            return new Vector2(x ?? target.x, y ?? target.y);
        }

        public static Vector3 Clone(Vector3 target, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? target.x, y ?? target.y, z ?? target.z);
        }

        public static Rect Clone(Rect target, float? x = null, float? y = null, float? width = null, float? height = null)
        {
            return new Rect(x ?? target.x, y ?? target.y, width ?? target.width, height ?? target.height);
        }

        public static Vector2 RightBottom(this Rect rect)
        {
            return new Vector2(rect.xMax, rect.yMax);
        }

        public static Vector2 RightTop(this Rect rect)
        {
            return new Vector2(rect.xMax, rect.yMin);
        }

        public static Vector2 LeftBottom(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMax);
        }

        public static Vector2 LeftTop(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }

        public static Vector2 Center(this Rect rect)
        {
            return new Vector2(rect.center.x, rect.center.y);
        }
    }
}