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

        [SerializeField] private StickmanGrid stickmanGrid;
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
                    tile.PlaceStickman(stickmanGrid.GetStickmanAt(x, y));

                    _tileGrid[x, y] = tile;
                    tile.name = $"Tile [{x},{y}]";
                }
            }
        }
   
        public bool AreNeighborsEmpty(int x, int y)
        {
            if (y == 0)
            {
                return true;
            }

            // Kom?uluk kontrolü yap
            bool up = y + 1 < _gridSize.y && !_tileGrid[x, y + 1].HasStickman();
            bool down = y - 1 >= 0 && !_tileGrid[x, y - 1].HasStickman();
            bool left = x - 1 >= 0 && !_tileGrid[x - 1, y].HasStickman();
            bool right = x + 1 < _gridSize.x && !_tileGrid[x + 1, y].HasStickman();
            return up || down || left || right;
        }

        public Tile GetTileAt(int x, int y)
        {
            // Geçerli bir koordinat olup olmad???n? kontrol et
            if (x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y)
            {
                return _tileGrid[x, y];
            }

            // Geçersiz koordinat durumunda null döndür
            Debug.LogWarning($"Tile at ({x}, {y}) is out of bounds.");
            return null;
        }


    }
}
