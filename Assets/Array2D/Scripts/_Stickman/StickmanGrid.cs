using UnityEngine;
using _Main._Enums;  // For ColorType
using LevelEditor;
using TriInspector;

namespace _Main._Stickman.StickmanGrid
{
    public class StickmanGrid : MonoBehaviour
    {
        [Title("Grid Configuration")]
        [SerializeField] private LevelDataSO _levelDataSO;
        [SerializeField] private Stickman[,] _stickmanGrid;
        [SerializeField] private Stickman _stickmanPrefab;

        private Vector2Int _gridSize;

        // Public property for GridSize
        public Vector2Int GridSize => _gridSize;

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
            _stickmanGrid = new Stickman[_gridSize.x, _gridSize.y];

            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    ColorType colorType = grid.GetCell(x, y);
                    if (colorType == ColorType._0None) continue;

                    int transformedY = _gridSize.y - 1 - y;

                    Vector3 position = new Vector3(x, 0, transformedY);

                    Stickman stickman = Instantiate(_stickmanPrefab, position, Quaternion.identity, transform);
                    stickman.UnitColorType = colorType;

                    // Stickman'a grid pozisyonunu ayarl?yoruz
                    stickman.SetGridPosition(x, y);

                    stickman.Initialize();

                    _stickmanGrid[x, y] = stickman;

                    stickman.name = $"Stickman [{x},{y}]";
                }
            }
        }

        // Stickman'a t?klama olay?n? dinlemek için yeni bir fonksiyon ekleyelim
        public void OnStickmanClicked(Stickman clickedStickman)
        {
            // T?klanan stickman'?n grid pozisyonunu al?yoruz
            int gridX = clickedStickman.GridX;
            int gridY = clickedStickman.GridY;

            // Bu pozisyonu debug log ile konsola yazd?r?yoruz
            Debug.Log($"Clicked Stickman is at grid position [{gridX}, {gridY}]");

            // Kom?u gridleri kontrol et
            CheckNeighbors(gridX, gridY);
        }

        // Kom?ular? kontrol etmek için metod
        private void CheckNeighbors(int x, int y)
        {
            // Sol kom?u
            CheckGridNeighbor(x - 1, y, "Left");
            // Sa? kom?u
            CheckGridNeighbor(x + 1, y, "Right");
            // A?a?? kom?u
            CheckGridNeighbor(x, y - 1, "Down");
            // Yukar? kom?u
            CheckGridNeighbor(x, y + 1, "Up");
        }

        // Grid pozisyonunun içinde stickman olup olmad???n? kontrol eden metod
        private void CheckGridNeighbor(int x, int y, string direction)
        {
            if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y)
            {
                Debug.Log($"{direction} neighbor grid [{x}, {y}] is outside the grid bounds.");
                return;
            }

            // E?er kom?u gridde bir Stickman varsa
            Stickman neighbor = _stickmanGrid[x, y];

            if (neighbor != null)
            {
                Debug.Log($"{direction} neighbor grid [{x}, {y}] is occupied by Stickman.");
            }
            else
            {
                Debug.Log($"{direction} neighbor grid [{x}, {y}] is empty.");
            }
        }

        // Stickman'? verilen x, y koordinat?nda alacak metod
        public Stickman GetStickmanAtPosition(int x, int y)
        {
            if (x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y)
            {
                return _stickmanGrid[x, y];
            }
            return null; // Geçersiz koordinat durumunda null dönecek
        }
    }
}
