using _Main._Stickman.StickmanGrid;
using LevelEditor;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    #region Fields
    [Header("Tile Grid Configuration")]
    [Tooltip("The prefab used to instantiate tiles in the grid.")]
    [SerializeField] private Tile _tilePrefab;

    [Tooltip("Reference to the StickmanGrid for Stickman assignments.")]
    [SerializeField] private StickmanGrid _stickmanGrid;

    private LevelDataSO _levelDataSO;
    private Tile[,] _tileGrid;
    private Vector2Int _gridSize;
    private bool _isInitialized;

    /// <summary>
    /// The size of the grid as a 2D vector.
    /// </summary>
    public Vector2Int GridSize => _gridSize;

    #endregion

    #region Public Methods

    /// <summary>
    /// Assigns the level data required to set up the grid.
    /// </summary>
    /// <param name="levelDataSO">The level data to set.</param>
    public void SetLevelDataSO(LevelDataSO levelDataSO)
    {
        if (levelDataSO == null)
            return;

        _levelDataSO = levelDataSO;
    }

    /// <summary>
    /// Initializes the grid if it hasn't been initialized yet.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized || _levelDataSO == null || _stickmanGrid == null)
            return;

        Setup(_levelDataSO.Array2DGrid);
        _isInitialized = true;
    }

    /// <summary>
    /// Sets up the tile grid using the provided Array2DGrid data.
    /// </summary>
    /// <param name="grid">The 2D grid data.</param>
    public void Setup(Array2DGrid grid)
    {
        if (grid == null)
        {
            Debug.LogError("Grid data is null!", this);
            return;
        }

        _gridSize = grid.GridSize;
        _tileGrid = new Tile[_gridSize.x, _gridSize.y];
        CreateTiles();
    }

    /// <summary>
    /// Checks if any neighboring tiles are empty.
    /// </summary>
    /// <param name="x">The x-coordinate of the tile.</param>
    /// <param name="y">The y-coordinate of the tile.</param>
    /// <returns>True if any neighbors are empty, otherwise false.</returns>
    public bool AreNeighborsEmpty(int x, int y)
    {
        if (y == 0) return true;

        bool up = IsNeighborEmpty(x, y + 1);
        bool down = IsNeighborEmpty(x, y - 1);
        bool left = IsNeighborEmpty(x - 1, y);
        bool right = IsNeighborEmpty(x + 1, y);

        return up || down || left || right;
    }

    /// <summary>
    /// Retrieves the tile at the specified coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the tile.</param>
    /// <param name="y">The y-coordinate of the tile.</param>
    /// <returns>The tile at the specified position, or null if invalid.</returns>
    public Tile GetTileAt(int x, int y)
    {
        if (!IsValidCoordinate(x, y))
        {
            Debug.LogWarning($"Tile at ({x}, {y}) is out of bounds.");
            return null;
        }
        return _tileGrid[x, y];
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates the tiles in the grid based on the grid size.
    /// </summary>
    private void CreateTiles()
    {
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
    /// Instantiates and initializes a tile at the specified position.
    /// </summary>
    /// <param name="x">The x-coordinate of the tile.</param>
    /// <param name="y">The y-coordinate of the tile.</param>
    /// <param name="position">The world position of the tile.</param>
    private void CreateTileAtPosition(int x, int y, Vector3 position)
    {
        if (_tilePrefab == null)
        {
            Debug.LogError("Tile prefab is missing!", this);
            return;
        }

        Tile tile = Instantiate(_tilePrefab, position, Quaternion.identity);
        tile.transform.SetParent(transform, worldPositionStays: false);
        tile.Initialize(position);

        if (_stickmanGrid != null)
        {
            Stickman stickman = _stickmanGrid.GetStickmanAt(x, y);
            if (stickman != null)
            {
                tile.AssignStickman(stickman);
            }
        }

        _tileGrid[x, y] = tile;
        tile.name = $"Tile [{x},{y}]";
    }

    /// <summary>
    /// Checks if the neighbor at the given position is empty.
    /// </summary>
    /// <param name="x">The x-coordinate of the neighbor.</param>
    /// <param name="y">The y-coordinate of the neighbor.</param>
    /// <returns>True if the neighbor is empty, otherwise false.</returns>
    private bool IsNeighborEmpty(int x, int y)
    {
        if (!IsValidCoordinate(x, y)) return false;
        return !_tileGrid[x, y].IsOccupied;
    }

    /// <summary>
    /// Validates if the given coordinates are within the grid bounds.
    /// </summary>
    /// <param name="x">The x-coordinate to check.</param>
    /// <param name="y">The y-coordinate to check.</param>
    /// <returns>True if the coordinates are valid, otherwise false.</returns>
    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y;
    }

    #endregion
}
