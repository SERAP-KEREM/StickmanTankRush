using LevelEditor;
using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class TileGrid : MonoBehaviour
    {
        [SerializeField] private GameObject _tilePrefab; // Tile prefab reference
        [SerializeField] private LevelDataSO _levelDataSO;
        private Tile[,] _tileGrid;  // 2D array of tiles in the grid
        private Vector2Int _gridSize;

        void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the grid setup.
        /// </summary>
        public void Initialize()
        {
            Setup(_levelDataSO.Array2DGrid);
        }

        /// <summary>
        /// Sets up the tile grid based on the provided 2D grid data.
        /// </summary>
        public void Setup(Array2DGrid grid)
        {
            _gridSize = grid.GridSize;
            _tileGrid = new Tile[_gridSize.x, _gridSize.y];

            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    Vector3 position = new Vector3(x, -1, y);

                    // Instantiate the tile and place it at the correct grid cell
                    GameObject tileObj = Instantiate(_tilePrefab, position, Quaternion.identity, transform);
                    Tile tile = tileObj.GetComponent<Tile>();
                    tile.Initialize(position);

                    _tileGrid[x, y] = tile;
                    tile.name = $"Tile [{x},{y}]";
                }
            }
        }

        /// <summary>
        /// Gets a specific tile at position (x, y).
        /// </summary>
        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y)
            {
                return _tileGrid[x, y];
            }
            return null;
        }

        /// <summary>
        /// Gets the world position of a specific tile.
        /// </summary>
        public Vector3 GetPosition(int x, int y)
        {
            Tile tile = GetTile(x, y);
            return tile != null ? tile.Position : Vector3.zero;
        }
    }
}
