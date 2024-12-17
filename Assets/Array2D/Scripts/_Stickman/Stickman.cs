using _Main._Enums;
using _Main._Stickman.StickmanGrid;
using UnityEngine;

    public class Stickman : MonoBehaviour
    {
        [SerializeField]
        private ColorType _colorType;
        public ColorType UnitColorType { get => _colorType; set => _colorType = value; }

        [SerializeField]
        private bool _isSelectable = true; // T?klanabilir yap?yoruz
        public bool IsSelectable { get => _isSelectable; set => _isSelectable = value; }

        // Grid pozisyonlar?
        public int GridX { get; private set; }
        public int GridY { get; private set; }

        // Grid pozisyonunu ayarlamak için metod
        public void SetGridPosition(int x, int y)
        {
            GridX = x;
            GridY = y;
        }

        // Stickman'? ba?latan fonksiyon
        public void Initialize()
        {
            Renderer childRenderer = transform.GetChild(0).GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.material.color = ColorManager.ColorTypeToColor(_colorType);
            }
        }

        // Mouse t?klama ile hangi Stickman'a t?kland???n? alg?la
        private void OnMouseDown()
        {
            // StickmanGrid'in OnStickmanClicked metodunu ça??r?yoruz
            StickmanGrid stickmanGrid = FindObjectOfType<StickmanGrid>(); // Sahnedeki StickmanGrid'i bul
            if (stickmanGrid != null)
            {
                // T?klanan Stickman'?n grid pozisyonunu bildiriyoruz
                stickmanGrid.OnStickmanClicked(this);
            }
        }
    }

