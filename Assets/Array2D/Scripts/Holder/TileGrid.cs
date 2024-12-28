using LevelEditor;
using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class TileGrid : MonoBehaviour
    {
        #region Fields

        [Header("Tile Grid Configuration")]
        [SerializeField, Tooltip("Tile prefab reference.")]
        private GameObject _tilePrefab; // Tile prefab reference

        [SerializeField, Tooltip("Level data for grid and tile configuration.")]
        private LevelDataSO _levelDataSO;

        [SerializeField, Tooltip("Stickman grid reference.")]
        private StickmanGrid stickmanGrid;

        private Tile[,] _tileGrid;  // 2D array of tiles in the grid
        private Vector2Int _gridSize;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the grid setup with the provided level data.
        /// </summary>
        public void Initialize()
        {
            Setup(_levelDataSO.Array2DGrid);
        }

        /// <summary>
        /// Sets up the tile grid based on the provided 2D grid data from the LevelDataSO.
        /// </summary>
        /// <param name="grid">2D grid data containing the level's grid configuration.</param>
        public void Setup(Array2DGrid grid)
        {
            _gridSize = grid.GridSize;
            _tileGrid = new Tile[_gridSize.x, _gridSize.y];

            // Create tiles based on the grid data
            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    Vector3 position = new Vector3(x, -1, y);
                    CreateTileAtPosition(x, y, position);
                }
            }
        }

        /// <summary>
        /// Checks if the neighboring tiles around the specified position are empty.
        /// </summary>
        /// <param name="x">X coordinate of the tile.</param>
        /// <param name="y">Y coordinate of the tile.</param>
        /// <returns>True if any neighbor is empty, false otherwise.</returns>
        public bool AreNeighborsEmpty(int x, int y)
        {
            if (y == 0) return true; // If it's the topmost row, it's always considered empty

            bool up = IsNeighborEmpty(x, y + 1);
            bool down = IsNeighborEmpty(x, y - 1);
            bool left = IsNeighborEmpty(x - 1, y);
            bool right = IsNeighborEmpty(x + 1, y);

            return up || down || left || right;
        }

        /// <summary>
        /// Retrieves the tile at the specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate of the tile.</param>
        /// <param name="y">Y coordinate of the tile.</param>
        /// <returns>The tile at the specified position, or null if out of bounds.</returns>
        public Tile GetTileAt(int x, int y)
        {
            if (IsValidCoordinate(x, y))
            {
                return _tileGrid[x, y];
            }

            Debug.LogWarning($"Tile at ({x}, {y}) is out of bounds.");
            return null;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Creates a tile at the specified position in the grid.
        /// </summary>
        /// <param name="x">X coordinate of the tile.</param>
        /// <param name="y">Y coordinate of the tile.</param>
        /// <param name="position">The position to instantiate the tile at.</param>
        private void CreateTileAtPosition(int x, int y, Vector3 position)
        {
            GameObject tileObj = Instantiate(_tilePrefab, position, Quaternion.identity, transform);
            Tile tile = tileObj.GetComponent<Tile>();
            tile.Initialize(position);
            tile.PlaceStickman(stickmanGrid.GetStickmanAt(x, y));

            _tileGrid[x, y] = tile;
            tile.name = $"Tile [{x},{y}]";
        }

        /// <summary>
        /// Checks if a specific neighbor tile is empty.
        /// </summary>
        /// <param name="x">X coordinate of the neighboring tile.</param>
        /// <param name="y">Y coordinate of the neighboring tile.</param>
        /// <returns>True if the tile is empty (no Stickman), otherwise false.</returns>
        private bool IsNeighborEmpty(int x, int y)
        {
            return IsValidCoordinate(x, y) && !_tileGrid[x, y].HasStickman();
        }

        /// <summary>
        /// Validates if the given coordinates are within the grid bounds.
        /// </summary>
        /// <param name="x">X coordinate of the tile.</param>
        /// <param name="y">Y coordinate of the tile.</param>
        /// <returns>True if the coordinates are within the grid bounds, false otherwise.</returns>
        private bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y;
        }

        #endregion
    }
}
