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
        [SerializeField] private LevelDataSO _levelDataSO; // Data for grid initialization
        [SerializeField] private Stickman _stickmanPrefab; // Prefab for instantiating Stickman units
        private Stickman[,] _stickmanGrid; // 2D array of Stickman units
        private Vector2Int _gridSize; // Grid dimensions

  

        [SerializeField, Tooltip("Reference to the TileGrid for positioning and interactions.")]
        private TileGrid _tileGrid;

     

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
        /// Returns the Stickman at the specified grid position.
        /// </summary>
        public Stickman GetStickmanAt(int x, int y)
        {
            return _stickmanGrid[x, y];
        }
    }
}
