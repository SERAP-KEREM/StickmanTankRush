using _Main._Enums;
using _Main._Stickman.StickmanGrid;
using UnityEngine;
using System.Collections;
using DG.Tweening;  // DOTween kullan?m? için ekledik

namespace _Main._Stickman.StickmanGrid
{
    public class Stickman : MonoBehaviour
    {
        [Header("Stickman Configuration")]
        [SerializeField]
        private ColorType _colorType;
        public ColorType UnitColorType
        {
            get => _colorType;
            set => _colorType = value;
        }

        [SerializeField, Tooltip("Determines if the Stickman is selectable.")]
        private bool _isSelectable = true;
        public bool IsSelectable
        {
            get => _isSelectable;
            set => _isSelectable = value;
        }

        // Grid position properties
        public int GridX { get; private set; }
        public int GridY { get; private set; }

        // Move speed (set to a low value for slower movement)
        [SerializeField, Tooltip("Move speed of the Stickman.")]
        private float moveSpeed = 1f;

        // Cache reference to the StickmanGrid for performance improvement
        private StickmanGrid stickmanGrid;

        private void Awake()
        {
            // Cache the StickmanGrid reference once during initialization
            stickmanGrid = FindObjectOfType<StickmanGrid>();
            if (stickmanGrid == null)
            {
                Debug.LogError("No StickmanGrid instance found in the scene.");
            }
        }

        // Sets the grid position of the Stickman.
        public void SetGridPosition(int x, int y)
        {
            GridX = x;
            GridY = y;
        }

        // Initializes the Stickman by setting its color based on the assigned color type.
        public void Initialize()
        {
            IsSelectable = true;
            Renderer childRenderer = transform.GetChild(0).GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.material.color = ColorManager.ColorTypeToColor(_colorType);
            }
            else
            {
                Debug.LogError("Renderer component not found on Stickman.");
            }
        }

        //// Detects mouse clicks and informs the StickmanGrid about the clicked Stickman.
        //private void OnMouseDown()
        //{
        //    if (stickmanGrid != null)
        //    {
        //        stickmanGrid.OnStickmanClicked(this);
        //    }
        //}

        // Moves the Stickman to the given target position (tank position) using DOTween.
        public void MoveToTank(Vector3 targetPosition)
        {
            if (stickmanGrid != null)
            {
                // DOTween kullanarak hareketi ba?lat?yoruz
                // targetPosition.z'yi negatif yap?yoruz
                targetPosition.z = -targetPosition.z; // Z de?erini tersine çeviriyoruz
                transform.DOMove(targetPosition, moveSpeed).SetEase(Ease.Linear).OnKill(() =>
                {
                    // Hareket tamamland???nda Stickman'? yok ediyoruz
                    Destroy(gameObject);
                });
            }
        }

        //// Removes the Stickman from the grid and destroys it from the scene.
        //public void RemoveStickmanFromGrid()
        //{
        //    if (stickmanGrid != null)
        //    {
        //        stickmanGrid.RemoveStickmanFromGrid(GridX, GridY); // Remove from grid
        //    }

        //    Destroy(gameObject); // Destroy from scene
        //}


    }
}
