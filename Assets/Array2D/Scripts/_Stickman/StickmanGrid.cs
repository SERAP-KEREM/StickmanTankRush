using _Main._Enums;
using UnityEngine;
using LevelEditor;

namespace _Main._Stickman.StickmanGrid
{
    /// <summary>
    /// Manages Stickman units on a grid, including interactions with tanks.
    /// </summary>
    public class StickmanGrid : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField]
        private LevelDataSO _levelDataSO; // Data for grid initialization
        [SerializeField]
        private Stickman _stickmanPrefab; // Prefab for instantiating Stickman units
        [SerializeField, Tooltip("Grid size and stickman layout.")]
        private Stickman[,] _stickmanGrid; // 2D array of Stickman units

        private Vector2Int _gridSize; // Grid dimensions
        private TankManager _tankManager; // Reference to TankManager

        public ColorType clickedStickmanColor; // Color of the clicked Stickman

        private void Start()
        {
            // Find and validate the TankManager instance
            _tankManager = FindObjectOfType<TankManager>();
            if (_tankManager == null)
            {
                Debug.LogError("TankManager instance not found in the scene.");
                return;
            }

            // Initialize the grid with the given level data
            Initialize();
        }

        /// <summary>
        /// Initializes the Stickman grid using LevelDataSO.
        /// </summary>
        public void Initialize()
        {
            if (_levelDataSO != null)
            {
                Setup(_levelDataSO.Array2DGrid);
            }
            else
            {
                Debug.LogError("LevelDataSO is not assigned!");
            }
        }

        /// <summary>
        /// Sets up the grid by instantiating Stickman units based on LevelDataSO grid information.
        /// </summary>
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

                    // Instantiate Stickman and initialize its properties
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
        /// Called when a stickman is clicked. Determines if the Stickman can board a tank.
        /// </summary>
        public void OnStickmanClicked(Stickman clickedStickman)
        {
            int gridX = clickedStickman.GridX;
            int gridY = clickedStickman.GridY;

            // Ignore click if Stickman has already boarded a tank
            if (_stickmanGrid[gridX, gridY] == null)
            {
                Debug.Log("This stickman has already boarded a tank and is no longer available.");
                return;
            }

            clickedStickmanColor = clickedStickman.UnitColorType;
            Debug.Log($"Clicked Stickman at position: [{gridX}, {gridY}], Color: {clickedStickmanColor}");

            // Yazd?rma: Kom?ular? kontrol et ve adlar?n? consola yazd?r
            PrintNeighborStickmen(gridX, gridY);

            // Try adding the Stickman to a tank
            TryAddStickmanToTank(clickedStickman);
        }

        /// <summary>
        /// Prints the names of the neighboring Stickmen (if any) to the console.
        /// </summary>
        private void PrintNeighborStickmen(int x, int y)
        {
            Stickman left = GetStickmanAt(x - 1, y);   // Corrected left-right coordinates
            Stickman right = GetStickmanAt(x + 1, y);  // Corrected left-right coordinates
            Stickman front = GetStickmanAt(x, y - 1);  // Corrected front-back coordinates
            Stickman back = GetStickmanAt(x, y + 1);   // Corrected front-back coordinates

            Debug.Log($"Neighbors of Stickman at [{x}, {y}]:");
            Debug.Log($"- Left: {(left != null ? left.name : "None")}");
            Debug.Log($"- Right: {(right != null ? right.name : "None")}");
            Debug.Log($"- Front: {(front != null ? front.name : "None")}");
            Debug.Log($"- Back: {(back != null ? back.name : "None")}");
        }

        /// <summary>
        /// Returns the Stickman at the specified grid position, or null if the position is empty or out of bounds.
        /// </summary>
        private Stickman GetStickmanAt(int x, int y)
        {
            if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y)
            {
                return null; // Out-of-bounds positions are considered empty
            }

            return _stickmanGrid[x, y];
        }

        /// <summary>
        /// Checks if the clicked Stickman can board a tank and moves it accordingly.
        /// </summary>
        private void TryAddStickmanToTank(Stickman clickedStickman)
        {
            if (_tankManager != null && _tankManager.GetCurrentTank() != null)
            {
                // Ensure the Stickman color matches the tank color
                if (_tankManager.GetCurrentTank().UnitColorType == clickedStickman.UnitColorType)
                {
                    // Check for empty neighboring grid cells where the Stickman can fit
                    if (AreNeighborsEmpty(clickedStickman.GridX, clickedStickman.GridY))
                    {
                        // Add Stickman to the tank
                        _tankManager.CheckAndAddStickmanToTank(clickedStickman.UnitColorType);

                        // Move Stickman towards the tank
                        clickedStickman.MoveToTank(_tankManager.GetCurrentTank().transform.position);
                    }
                    else
                    {
                        Debug.Log("No empty space for the stickman to board the tank.");
                    }
                }
                else
                {
                    Debug.Log("Stickman color does not match the tank color!");
                }
            }
            else
            {
                Debug.LogError("No active tank available to board.");
            }
        }

        /// <summary>
        /// Checks if any neighboring grid cells are empty.
        /// </summary>
        public bool AreNeighborsEmpty(int x, int y)
        {
            bool leftEmpty = IsGridEmpty(x - 1, y);   // Corrected coordinates
            bool rightEmpty = IsGridEmpty(x + 1, y);  // Corrected coordinates
            bool frontEmpty = IsGridEmpty(x, y - 1);  // Corrected coordinates
            bool backEmpty = IsGridEmpty(x, y + 1);   // Corrected coordinates

            return leftEmpty || rightEmpty || frontEmpty || backEmpty;
        }

        /// <summary>
        /// Checks if a specific grid cell is empty or out-of-bounds.
        /// </summary>
        private bool IsGridEmpty(int x, int y)
        {
            // Out-of-bounds cells are considered empty
            if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y)
            {
                return true;
            }

            Stickman neighbor = _stickmanGrid[x, y];
            return neighbor == null; // If there's no Stickman, the cell is empty
        }

        /// <summary>
        /// Removes the Stickman from the grid at the specified position.
        /// </summary>
        public void RemoveStickmanFromGrid(int x, int y)
        {
            if (_stickmanGrid[x, y] != null)
            {
                _stickmanGrid[x, y] = null; // Set grid position to null when Stickman is removed
            }
        }

        /// <summary>
        /// Visualizes the Stickman units in the Editor using Gizmos.
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
                        Gizmos.DrawSphere(stickman.transform.position, 0.1f);
                    }
                }
            }
        }
    }
}
