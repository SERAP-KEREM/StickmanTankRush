using _Main._Enums;
using UnityEngine;
using LevelEditor;

/// <summary>
/// Manages the grid of Stickman units. Responsible for initializing the grid and handling interactions with Stickman objects.
/// </summary>
namespace _Main._Stickman.StickmanGrid
{
    public class StickmanGrid : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField]
        private LevelDataSO _levelDataSO; // Data for grid initialization
        [SerializeField]
        private Stickman[,] _stickmanGrid; // 2D array of Stickman units
        [SerializeField]
        private Stickman _stickmanPrefab; // Prefab for instantiating Stickman units

        private Vector2Int _gridSize; // Grid dimensions

        // Public property to expose grid size
        public Vector2Int GridSize => _gridSize;

        // Stores the color of the clicked Stickman
        public ColorType clickedStickmanColor;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the grid with data from LevelDataSO.
        /// </summary>
        public void Initialize()
        {
            Setup(_levelDataSO.Array2DGrid);
        }

        /// <summary>
        /// Sets up the grid with the provided Array2DGrid data.
        /// </summary>
        /// <param name="grid">The grid data to use for setup.</param>
        public void Setup(Array2DGrid grid)
        {
            _gridSize = grid.GridSize;
            _stickmanGrid = new Stickman[_gridSize.x, _gridSize.y];

            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    ColorType colorType = grid.GetCell(x, y);
                    if (colorType == ColorType._0None) continue; // Skip empty cells

                    Vector3 position = new Vector3(x, 0, y);
                    Stickman stickman = Instantiate(_stickmanPrefab, position, Quaternion.identity, transform);
                    stickman.UnitColorType = colorType;
                    stickman.SetGridPosition(x, y);
                    stickman.Initialize();

                    _stickmanGrid[x, y] = stickman;
                    stickman.name = $"Stickman [{x},{y}]";
                }
            }
        }

        /// <summary>
        /// Handles the event when a Stickman is clicked.
        /// </summary>
        /// <param name="clickedStickman">The Stickman that was clicked.</param>
        public void OnStickmanClicked(Stickman clickedStickman)
        {
            int gridX = clickedStickman.GridX;
            int gridY = clickedStickman.GridY;
            clickedStickmanColor = clickedStickman.UnitColorType;
            Debug.Log($"Clicked Stickman at position: [{gridX}, {gridY}], Color: {clickedStickmanColor}");

            // Check the neighbors of the clicked Stickman
            CheckNeighbors(gridX, gridY);
        }

        /// <summary>
        /// Checks the neighboring cells of the clicked Stickman.
        /// </summary>
        private void CheckNeighbors(int x, int y)
        {
            CheckGridNeighbor(x, y - 1, "Left");
            CheckGridNeighbor(x, y + 1, "Right");
            CheckGridNeighbor(x - 1, y, "Front");
            CheckGridNeighbor(x + 1, y, "Back");
        }

        /// <summary>
        /// Checks a specific neighboring cell and logs its status (empty or filled).
        /// </summary>
        /// <param name="x">The x-coordinate of the neighbor.</param>
        /// <param name="y">The y-coordinate of the neighbor.</param>
        /// <param name="direction">The direction (e.g., Left, Right) for logging.</param>
        private void CheckGridNeighbor(int x, int y, string direction)
        {
            // Avoid checking out-of-bounds cells
            if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y)
            {
                Debug.Log($"{direction} neighbor [{x}, {y}] is out of bounds.");
                return;
            }

            // Get the neighbor Stickman
            Stickman neighbor = _stickmanGrid[x, y];
            if (neighbor != null)
            {
                Debug.Log($"{direction} neighbor [{x}, {y}] is filled with Stickman. Color: {neighbor.UnitColorType}");
            }
            else
            {
                Debug.Log($"{direction} neighbor [{x}, {y}] is empty.");
            }
        }

        /// <summary>
        /// Retrieves the Stickman at the specified position.
        /// </summary>
        /// <param name="x">X position on the grid</param>
        /// <param name="y">Y position on the grid</param>
        /// <returns>The Stickman at the specified position or null if empty.</returns>
        public Stickman GetStickmanAtPosition(int x, int y)
        {
            if (x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y)
            {
                return _stickmanGrid[x, y];
            }
            return null;
        }

        /// <summary>
        /// Draws Gizmos for debugging in the Unity Editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_stickmanGrid != null)
            {
                foreach (var stickman in _stickmanGrid)
                {
                    if (stickman != null)
                    {
                        Gizmos.color = ColorManager.ColorTypeToColor(stickman.UnitColorType);
                        Gizmos.DrawSphere(stickman.transform.position, 0.1f); // Draw a sphere for each Stickman
                    }
                }
            }
        }
    }
}
