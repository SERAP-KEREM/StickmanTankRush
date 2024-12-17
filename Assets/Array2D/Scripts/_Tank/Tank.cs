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
            Color newColor = ColorManager.ColorTypeToColor(_colorType);

            // Bu objenin ve tüm child objelerin Renderer bile?enlerine eri?
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = newColor;
            }
        }
    }
}
