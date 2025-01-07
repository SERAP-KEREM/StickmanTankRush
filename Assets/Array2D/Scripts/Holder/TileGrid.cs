using _Main._Stickman.StickmanGrid;
using LevelEditor;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    #region Fields


    [Header("Tile Grid Configuration")]
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private StickmanGrid _stickmanGrid;

    private LevelDataSO _levelDataSO;
    private Tile[,] _tileGrid;
    private Vector2Int _gridSize;
    private bool _isInitialized;
    #endregion


    #region Public Methods
    public void SetLevelDataSO(LevelDataSO levelDataSO)
    {
        if (levelDataSO == null)
        {
            Debug.LogError("Trying to set null LevelDataSO!", this);
            return;
        }

        _levelDataSO = levelDataSO;
    }

    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("TileGrid is already initialized!");
            return;
        }

        if (_levelDataSO == null)
        {
            Debug.LogError("LevelDataSO is not assigned in TileGrid.");
            return;
        }

        if (_stickmanGrid == null)
        {
            Debug.LogError("StickmanGrid reference is missing!", this);
            return;
        }

        Setup(_levelDataSO.Array2DGrid);
        _isInitialized = true;
    }

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

    public bool AreNeighborsEmpty(int x, int y)
    {
        if (y == 0) return true;

        bool up = IsNeighborEmpty(x, y + 1);
        bool down = IsNeighborEmpty(x, y - 1);
        bool left = IsNeighborEmpty(x - 1, y);
        bool right = IsNeighborEmpty(x + 1, y);

        return up || down || left || right;
    }

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

    private bool IsNeighborEmpty(int x, int y)
    {
        if (!IsValidCoordinate(x, y)) return false;
        return !_tileGrid[x, y].IsOccupied;
    }

    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y;
    }
    #endregion
}