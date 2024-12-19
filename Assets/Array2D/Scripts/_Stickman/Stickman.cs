using _Main._Enums;
using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    /// <summary>
    /// Represents a Stickman unit on the grid.
    /// Responsible for initializing the Stickman, setting its position, and detecting mouse clicks.
    /// </summary>
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

        /// <summary>
        /// Sets the grid position of the Stickman.
        /// </summary>
        /// <param name="x">X position on the grid</param>
        /// <param name="y">Y position on the grid</param>
        public void SetGridPosition(int x, int y)
        {
            GridX = x;
            GridY = y;
        }

        /// <summary>
        /// Initializes the Stickman by setting its color based on the assigned color type.
        /// </summary>
        public void Initialize()
        {
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

        /// <summary>
        /// Detects mouse clicks and informs the StickmanGrid about the clicked Stickman.
        /// </summary>
        private void OnMouseDown()
        {
            StickmanGrid stickmanGrid = FindObjectOfType<StickmanGrid>(); // Find the StickmanGrid instance in the scene
            if (stickmanGrid != null)
            {
                stickmanGrid.OnStickmanClicked(this);
            }
            else
            {
                Debug.LogError("No StickmanGrid instance found in the scene.");
            }
        }
    }
}
