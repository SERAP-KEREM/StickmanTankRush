using UnityEngine;

namespace _Main._Enums
{
    public enum ColorType
    {
        _0None,
        _1Green,
        _2Blue,
        _3Red,
        _4Yellow,
        _5Purple,
        _6Pink,
        _7Orange
    }
    public static class ColorManager
    {
        public static Color ColorTypeToColor(ColorType colorType)
        {
            switch (colorType)
            {
                case ColorType._1Green:
                    return Color.green;
                case ColorType._2Blue:
                    return Color.blue;
                case ColorType._3Red:
                    return Color.red;
                case ColorType._4Yellow:
                    return Color.yellow;
                case ColorType._5Purple:
                    return new Color(0.5f, 0, 0.5f);
                case ColorType._6Pink:
                    return new Color(1f, 0.4f, 0.7f);
                case ColorType._7Orange:
                    return new Color(1f, 0.5f, 0f);
                default:
                    return Color.clear; 
            }
        }
    }
}

