using _Main._Enums;
using UnityEngine;
using DG.Tweening;  // Added for DOTween functionality

namespace _Main._Stickman.StickmanGrid
{
    /// <summary>
    /// Represents a Stickman character that can move and interact within the grid.
    /// </summary>
    public class Stickman : MonoBehaviour
    {
        #region Fields & Properties

        [Header("Stickman Configuration")]
        [SerializeField, Tooltip("Determines the color type of the Stickman.")]
        private ColorType _colorType;

        [SerializeField, Tooltip("Indicates if the Stickman is selectable.")]
        private bool _isSelectable = true;

        [SerializeField, Range(0.1f, 5f), Tooltip("Move speed of the Stickman.")]
        private float _moveSpeed = 1f;

        private StickmanGrid _stickmanGrid;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the color type of the Stickman.
        /// </summary>
        public ColorType UnitColorType
        {
            get => _colorType;
            set => _colorType = value;
        }

        /// <summary>
        /// Gets or sets whether the Stickman is selectable.
        /// </summary>
        public bool IsSelectable
        {
            get => _isSelectable;
            set => _isSelectable = value;
        }

        /// <summary>
        /// Gets the current grid X position of the Stickman.
        /// </summary>
        public int GridX { get; private set; }

        /// <summary>
        /// Gets the current grid Y position of the Stickman.
        /// </summary>
        public int GridY { get; private set; }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            // Cache StickmanGrid reference for performance optimization
            _stickmanGrid = FindObjectOfType<StickmanGrid>();
            if (_stickmanGrid == null)
            {
                Debug.LogError("No StickmanGrid instance found in the scene.");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the grid position of the Stickman.
        /// </summary>
        /// <param name="x">X position in the grid.</param>
        /// <param name="y">Y position in the grid.</param>
        public void SetGridPosition(int x, int y)
        {
            GridX = x;
            GridY = y;
        }

        /// <summary>
        /// Initializes the Stickman by setting its color based on its assigned color type.
        /// </summary>
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

        /// <summary>
        /// Moves the Stickman to the specified target position using DOTween.
        /// </summary>
        /// <param name="targetPosition">The target position where the Stickman will move.</param>
        /// <param name="tankTransform">The tank's transform to attach the Stickman to after movement.</param>
        public void MoveToTank(Vector3 targetPosition, Transform tankTransform)
        {
            if (_stickmanGrid != null)
            {
                targetPosition.z = -targetPosition.z; // Ensure the z-axis is flipped (if necessary for your setup)
                MoveToPosition(targetPosition, tankTransform);
            }
        }

        /// <summary>
        /// Moves the Stickman to a specified holder position.
        /// </summary>
        /// <param name="holderPosition">The target holder position.</param>
        public void MoveToHolder(Vector3 holderPosition)
        {
            Vector3 targetPosition = holderPosition;
            targetPosition.y = transform.position.y; // Maintain current Y position
            MoveToPosition(targetPosition);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// General method for moving the Stickman to a specified position.
        /// </summary>
        /// <param name="targetPosition">The target position where the Stickman will move.</param>
        /// <param name="tankTransform">Optional: The tank's transform to attach the Stickman to after movement.</param>
        private void MoveToPosition(Vector3 targetPosition, Transform tankTransform = null)
        {
            transform.DOMove(targetPosition, _moveSpeed)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (tankTransform != null)
                    {
                        transform.SetParent(tankTransform);
                        IsSelectable = false;
                    }
                });
        }

        #endregion
    }
}
