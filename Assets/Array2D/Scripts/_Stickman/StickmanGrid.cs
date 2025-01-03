using _Main._Enums;
using UnityEngine;
using LevelEditor;
using System.Collections.Generic;

namespace _Main._Stickman.StickmanGrid
{
    public class StickmanGrid : MonoBehaviour
    {
        #region Fields & Properties
        public static StickmanGrid Instance { get; private set; }

        [Header("Grid Configuration")]
        [SerializeField] private Stickman _stickmanPrefab;
        [SerializeField] private TileGrid _tileGrid;
        [SerializeField] private Vector3 _gridOffset = Vector3.zero; // Grid'in ba?lang?ç pozisyonu için offset

        private LevelDataSO _levelDataSO;
        private Stickman[,] _stickmanGrid;
        private Vector2Int _gridSize;

        public Vector2Int GridSize => _gridSize;

        // Event system for grid changes
        public delegate void GridEventHandler(Vector2Int position, Stickman stickman);
        public event GridEventHandler OnStickmanPlaced;
        public event GridEventHandler OnStickmanRemoved;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad kald?r?ld? çünkü her level için yeni bir grid olu?turulmal?
        }

        private void Start()
        {
            if (_levelDataSO != null)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            ClearGrid();
        }
        #endregion

        #region Grid Management
        public void SetLevelDataSO(LevelDataSO levelDataSO)
        {
            if (levelDataSO == null)
            {
                Debug.LogError("Trying to set null LevelDataSO!", this);
                return;
            }

            _levelDataSO = levelDataSO;
            Initialize();
        }

        public void Initialize()
        {
            if (!ValidateSetup()) return;

            ClearGrid();
            Setup(_levelDataSO.Array2DGrid);
        }

        private bool ValidateSetup()
        {
            if (_levelDataSO == null)
            {
                Debug.LogError("LevelDataSO is not assigned!", this);
                return false;
            }

            if (_stickmanPrefab == null)
            {
                Debug.LogError("Stickman prefab is missing!", this);
                return false;
            }

            if (_tileGrid == null)
            {
                Debug.LogError("TileGrid reference is missing!", this);
                return false;
            }

            return true;
        }

        private void Setup(Array2DGrid grid)
        {
            _gridSize = grid.GridSize;
            _stickmanGrid = new Stickman[_gridSize.x, _gridSize.y];

            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    CreateStickmanIfNeeded(x, y, grid.GetCell(x, y));
                }
            }
        }

        private void CreateStickmanIfNeeded(int x, int y, ColorType colorType)
        {
            if (colorType == ColorType._0None) return;

            Vector3 position = CalculateWorldPosition(x, y);
            Stickman stickman = InstantiateStickman(position, x, y, colorType);

            _stickmanGrid[x, y] = stickman;
            OnStickmanPlaced?.Invoke(new Vector2Int(x, y), stickman);
        }

        private Vector3 CalculateWorldPosition(int x, int y)
        {
            return new Vector3(x, 0, y) + _gridOffset;
        }

        private Stickman InstantiateStickman(Vector3 position, int x, int y, ColorType colorType)
        {
            Stickman stickman = Instantiate(_stickmanPrefab, position, Quaternion.identity);
            stickman.transform.SetParent(transform, worldPositionStays: false);
            stickman.UnitColorType = colorType;
            stickman.SetGridPosition(x, y);
            stickman.Initialize();
            stickman.name = $"Stickman [{x},{y}]";
            return stickman;
        }

        private void ClearGrid()
        {
            if (_stickmanGrid == null) return;

            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    if (_stickmanGrid[x, y] != null)
                    {
                        OnStickmanRemoved?.Invoke(new Vector2Int(x, y), _stickmanGrid[x, y]);
                        Destroy(_stickmanGrid[x, y].gameObject);
                        _stickmanGrid[x, y] = null;
                    }
                }
            }
        }
        #endregion

        #region Grid Queries
        public Stickman GetStickmanAt(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return _stickmanGrid[x, y];
        }

        public Stickman GetStickmanAt(Vector2Int position)
        {
            return GetStickmanAt(position.x, position.y);
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y;
        }

        public bool HasStickmanAt(int x, int y)
        {
            return IsValidPosition(x, y) && _stickmanGrid[x, y] != null;
        }
        #endregion

        #region Debug
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _stickmanGrid == null) return;

            Gizmos.color = Color.yellow;
            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    if (HasStickmanAt(x, y))
                    {
                        Vector3 pos = CalculateWorldPosition(x, y);
                        Gizmos.DrawWireCube(pos, Vector3.one);
                    }
                }
            }
        }
        #endregion
    }
}