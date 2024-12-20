using _Main._Enums;
using UnityEngine;
using System.Collections;

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
            if (stickmanGrid != null)
            {
                stickmanGrid.OnStickmanClicked(this);
            }
        }

        /// <summary>
        /// Moves the Stickman to the given target position (tank position).
        /// </summary>
        public void MoveToTank(Vector3 targetPosition)
        {
            if (stickmanGrid != null)
            {
                StartCoroutine(MoveTowardsTank(targetPosition));
            }
        }

        private IEnumerator MoveTowardsTank(Vector3 targetPosition)
        {
            // Smooth movement towards the tank position
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Once the stickman reaches the tank, destroy it
            Destroy(gameObject);
        }

        /// <summary>
        /// Removes the Stickman from the grid and destroys it from the scene.
        /// </summary>
        public void RemoveStickmanFromGrid()
        {
            if (stickmanGrid != null)
            {
                stickmanGrid.RemoveStickmanFromGrid(GridX, GridY); // Remove from grid
            }

            Destroy(gameObject); // Destroy from scene
        }
    }
}
