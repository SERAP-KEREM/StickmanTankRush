using LevelEditor;
using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class TileGrid : MonoBehaviour
    {
        [SerializeField] private GameObject _tilePrefab; // Tile prefab'?
        [SerializeField] private LevelDataSO _levelDataSO;
        private Tile[,] _tileGrid;  // Tüm gridin tile'lar?
        private Vector2Int _gridSize;

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            Setup(_levelDataSO.Array2DGrid);
        }

        public void Setup(Array2DGrid grid)
        {
            _gridSize = grid.GridSize;
            _tileGrid = new Tile[_gridSize.x, _gridSize.y];

            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    // Grid hücresine kar??l?k gelen pozisyon
                    Vector3 position = new Vector3(x, -1, y);

                    // Tile'? instantiate et ve griddeki uygun hücreye yerle?tir
                    GameObject tileObj = Instantiate(_tilePrefab, position, Quaternion.identity, transform);
                    Tile tile = tileObj.GetComponent<Tile>();
                    tile.Initialize(position);

                    _tileGrid[x, y] = tile;
                }
            }
        }

        // Belirli bir tile'? al
        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y)
            {
                return _tileGrid[x, y];
            }
            return null;
        }

        // Belirli bir tile'?n pozisyonunu al
        public Vector3 GetPosition(int x, int y)
        {
            Tile tile = GetTile(x, y);
            return tile != null ? tile.Position : Vector3.zero;
        }
    }
}
