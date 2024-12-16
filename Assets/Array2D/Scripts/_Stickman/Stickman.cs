using System.Collections;
using _Main._Enums;
using TriInspector;
using UnityEngine;
using UnityEngine.AI;

namespace _Main._Stickman
{
    public class Stickman : MonoBehaviour
    {
        [Title("ColorType")]
        [SerializeField]
        private ColorType _colorType;
        public ColorType UnitColorType { get => _colorType; set => _colorType = value; }

        [Title("Inner")]
        [SerializeField]
        private bool _isMoving;
        public bool IsMoving { get => _isMoving; }

        [SerializeField]
        private bool _isSelectable = false;
        public bool IsSelectable { get => _isSelectable; set => _isSelectable = value; }

        // Yeni eklenen grid koordinatlar?
        public int GridX { get; private set; }
        public int GridY { get; private set; }

        public void Initialize()
        {
            Renderer childRenderer = transform.GetChild(0).GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.material.color = ColorTypeToColor(_colorType);
            }
        }

        // Grid pozisyonunu ayarlamak i�in bir metod
        public void SetGridPosition(int x, int y)
        {
            GridX = x;
            GridY = y;
          //  Debug.Log($"Stickman at position [{x}, {y}]");
        }

        public IEnumerator Move(Transform moveTransform)
        {
            _isMoving = true;
            yield return null;
            _isMoving = false;
        }

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
                    return Color.clear;
            }
        }
    }
}
