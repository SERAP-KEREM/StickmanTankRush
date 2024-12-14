using UnityEngine;
using _Main._Enums; // For ColorType
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

                    // Y ekseninde gridSize.y - 1 - y ile pozisyon belirleme
                    int transformedY = _gridSize.y - 1 - y;

                    Vector3 position = new Vector3(x, 0, transformedY);

                    Stickman stickman = Instantiate(_stickmanPrefab, position, Quaternion.identity, transform);
                    stickman.UnitColorType = colorType;

                    stickman.Initialize();

                    _stickmanGrid[x, y] = stickman;

                    stickman.name = $"Stickman [{x},{y}]";
                }
            }
        }
    }
}