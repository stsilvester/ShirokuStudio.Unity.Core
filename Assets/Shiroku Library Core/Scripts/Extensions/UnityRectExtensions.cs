using UnityEngine;

namespace ShirokuStudio.Core
{
    public enum VerticalAlignmentType
    {
        Upper = AlignmentType.Upper,
        Lower = AlignmentType.Lower,
        Middle = AlignmentType.Middle,
    }

    public enum HorizontalAlignmentType
    {
        Left = AlignmentType.Left,
        Right = AlignmentType.Right,
        Center = AlignmentType.Center,
    }

    public enum AlignmentType
    {
        Left = 1,
        Right = 2,
        Center = 3,

        Upper = 0x10,
        Lower = 0x20,
        Middle = 0x30,

        LeftUpper = Left | Upper,
        LeftMiddle = Left | Middle,
        LeftLower = Left | Lower,

        RightUpper = Right | Upper,
        RightMiddle = Right | Middle,
        RightLower = Right | Lower,

        CenterUpper = Center | Upper,
        CenterMiddle = Center | Middle,
        CenterLower = Center | Lower,
    }

    public static class UnityRectExtensions
    {
        private const AlignmentType AlignmentAxis_Horizontal = (AlignmentType)0xF;
        private const AlignmentType AlignmentAxis_Vertical = (AlignmentType)0xF0;

        public static Rect Align(this Rect outerRect, AlignmentType alignment, Rect innerRect)
        {
            if ((alignment & AlignmentAxis_Horizontal) != 0)
            {
                switch (alignment & AlignmentAxis_Horizontal)
                {
                    case AlignmentType.Left:
                        innerRect.x = outerRect.x;
                        break;

                    case AlignmentType.Center:
                        innerRect.x = outerRect.x + (outerRect.width - innerRect.width) / 2;
                        break;

                    case AlignmentType.Right:
                        innerRect.x = outerRect.x + outerRect.width - innerRect.width;
                        break;
                }
            }

            if ((alignment & AlignmentAxis_Vertical) != 0)
            {
                switch (alignment & AlignmentAxis_Vertical)
                {
                    case AlignmentType.Upper:
                        innerRect.y = outerRect.y;
                        break;

                    case AlignmentType.Middle:
                        innerRect.y = outerRect.y + (outerRect.height - innerRect.height) / 2;
                        break;

                    case AlignmentType.Lower:
                        innerRect.y = outerRect.y + outerRect.height - innerRect.height;
                        break;
                }
            }

            return innerRect;
        }

        public static Rect Align(this Rect outerRect, AlignmentType alignment, Vector2 size)
        {
            return outerRect.Align(alignment, new Rect(0, 0, size.x, size.y));
        }

        public static Rect AlignVertically(this Rect outer, VerticalAlignmentType alignment, float height)
        {
            return outer.Align((AlignmentType)alignment | AlignmentType.Center, new Rect(0, 0, outer.width, height));
        }

        public static Rect AlignHorizontally(this Rect outer, HorizontalAlignmentType alignment, float width)
        {
            return outer.Align((AlignmentType)alignment | AlignmentType.Middle, new Rect(0, 0, width, outer.height));
        }

        public static Rect OffsetSelf(this ref Rect rect, float x, float y)
            => rect = Offset(rect, x, y);

        public static Rect Offset(this Rect rect, float x, float y)
        {
            rect.x += x;
            rect.y += y;
            return rect;
        }
    }
}