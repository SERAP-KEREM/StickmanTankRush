using System.Collections.Generic;
using _Main._Enums;
using UnityEngine;

namespace _Main._Tank
{
    public class Tank : MonoBehaviour
    {
        [SerializeField]
        private ColorType _colorType;
        public ColorType UnitColorType
        {
            get => _colorType;
            set
            {
                _colorType = value;
                UpdateColor();
            }
        }

        // Tank olu?turuldu?unda rengi ayarlar
        public void Initialize()
        {
            UpdateColor();
        }

        // Tüm child objelerin renklerini de?i?tirme i?lemi
        private void UpdateColor()
        {
            // ColorType'a göre yeni renk belirle
            Color newColor = ColorTypeToColor(_colorType);

            // Bu objenin ve tüm child objelerin Renderer bile?enlerine eri?
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = newColor;
            }
        }

        // ColorType'? Unity renklerine çeviren yard?mc? fonksiyon
        private Color ColorTypeToColor(ColorType colorType)
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
                    return Color.gray; // Default renk
            }
        }
    }
}
