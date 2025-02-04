using UnityEngine;

namespace ShirokuStudio.Core
{
    public struct RichText
    {
        public string Text;
        public Color? Color;
        public bool Bold;
        public bool Italic;
        public int? Size;
        public RichLink? Link;

        public RichText(object text,
            Color? color = null,
            bool bold = false,
            bool italic = false,
            int? size = null,
            RichLink? link = null)
        {
            Text = text?.ToString() ?? "";
            Color = color;
            Bold = bold;
            Italic = italic;
            Size = size;
            Link = link;
        }

        public RichText SetColor(Color color)
        {
            Color = color;
            return this;
        }

        public RichText SetBold(bool value = true)
        {
            Bold = value;
            return this;
        }

        public RichText SetItalic(bool value = true)
        {
            Italic = value;
            return this;
        }

        public RichText SetSize(int size)
        {
            Size = size;
            return this;
        }

        public override string ToString()
        {
            var result = Text;
            if (Color.HasValue)
                result = $"<color=#{ColorUtility.ToHtmlStringRGBA(Color.Value)}>{result}</color>";

            if (Bold)
                result = $"<b>{result}</b>";

            if (Italic)
                result = $"<i>{result}</i>";

            if (Link.HasValue)
                result = $"<a {Link}>{result}</a>";

            return result;
        }

        public static implicit operator string(RichText richText)
        {
            return richText.ToString();
        }
    }

    public struct RichLink
    {
        public enum LinkType
        {
            Href,
            Command
        }

        public string Link;
        public LinkType Type;

        public RichLink(LinkType type, string link)
        {
            Type = type;
            Link = link;
        }

        public static string GetAttr(LinkType type)
        {
            switch (type)
            {
                default:
                case LinkType.Href:
                    return "href";

                case LinkType.Command:
                    return "cmd";
            }
        }

        public override string ToString()
        {
            return $"{GetAttr(Type)}=\"{Link}\"";
        }
    }
}