using ShirokuStudio.Core.Reflection;
using System;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static class GUIStyleUtility
    {
        public static Texture2D CreateTex(int width, int height, Color color)
        {
            var pixels = new Color[width * height];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            var result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        public static Texture2D CreateBorderedTex(int border, Color contentColor, Color borderColor)
        {
            var size = new Vector2Int(border * 4, border * 4);
            var rect = new RectOffset(border, border, border, border);
            return CreateBorderedTex(size, rect, contentColor, borderColor);
        }

        public static Texture2D CreateBorderedTex(int width, int height, int border, Color contentColor, Color borderColor)
        {
            var size = new Vector2Int(width, height);
            var rect = new RectOffset(border, border, border, border);
            return CreateBorderedTex(size, rect, contentColor, borderColor);
        }

        public static Texture2D CreateBorderedTex(Vector2Int contentSize, RectOffset borderOffset, Color contentColor, Color borderColor)
        {
            var width = contentSize.x + borderOffset.left + borderOffset.right;
            var height = contentSize.y + borderOffset.top + borderOffset.bottom;
            var pixels = new Color[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                var x = i % width;
                var y = i / width;
                if (x < borderOffset.left || x >= width - borderOffset.right || y < borderOffset.top || y >= height - borderOffset.bottom)
                    pixels[i] = borderColor;
                else
                    pixels[i] = contentColor;
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        public static GUIStyle GetCachedGUIStyle(object key, GUIStyle src, Action<GUIStyle> onCreate = null)
        {
            return FastCacher<(object key, GUIStyle src), GUIStyle>
                .Get((key, src), args =>
                {
                    var style = new GUIStyle(args.src);
                    onCreate?.Invoke(style);
                    return style;
                });
        }

        public static GUIStyle GetCachedGUIStyle(
            object key,
            Action<GUIStyle> onCreate = null)
        {
            return GetCachedGUIStyle(key, "", onCreate);
        }

        public static GUIStyle GetCachedGUIStyle(
            object key,
            string src,
            Action<GUIStyle> onCreate = null)
        {
            src ??= "";
            return FastCacher<(object key, string src), GUIStyle>.Get(
                (key, src), args =>
                {
                    var style = new GUIStyle(args.src);
                    onCreate?.Invoke(style);
                    return style;
                });
        }

        public static GUIStyle GetCachedGUIStyle<T>(
            object key,
            string src,
            T data,
            Action<GUIStyle, T> onCreate)
        {
            src ??= "";
            return FastCacher<(object key, string src, T data), GUIStyle>
                .Get((key, src, data), args =>
                {
                    var style = new GUIStyle(args.src);
                    onCreate?.Invoke(style, args.data);
                    return style;
                });
        }
    }
}