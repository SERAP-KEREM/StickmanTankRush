using _Main._Enums;
using UnityEngine;
using LevelEditor;
using System.Collections.Generic;
using System.Collections;

namespace _Main._Stickman.StickmanGrid
{
    /// <summary>
    /// Manages the placement and removal of Stickman units on a grid.
    /// </summary>
    public class StickmanGrid : MonoBehaviour
    {
        #region Fields & Properties

        [Header("Grid Configuration")]
        [SerializeField] private Stickman _stickmanPrefab; // Prefab for Stickman.
        [SerializeField] private TileGrid _tileGrid; 
        [SerializeField] private Vector3 _gridOffset = Vector3.zero; 

        private LevelDataSO _levelDataSO; 
        private Stickman[,] _stickmanGrid; 
        private Vector2Int _gridSize; 

        public delegate void GridEventHandler(Vector2Int position, Stickman stickman);
        public event GridEventHandler OnStickmanPlaced; // Event triggered when a Stickman is placed.
        public event GridEventHandler OnStickmanRemoved; // Event triggered when a Stickman is removed.

        #endregion

        private bool _isInitialized; 

        #region Unity Lifecycle

        /// <summary>
        /// Clears the grid when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            ClearGrid();
        }
        #endregion

        #region Grid Management

        /// <summary>
        /// Sets the LevelDataSO for the grid.
        /// </summary>
        public void SetLevelDataSO(LevelDataSO levelDataSO)
        {
            if (levelDataSO == null)
            {
                Debug.LogError("Trying to set null LevelDataSO!", this);
                return;
            }

            _levelDataSO = levelDataSO;
        }

        /// <summary>
        /// Initializes the Stickman grid based on the level data.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            if (!ValidateSetup()) return;

            Debug.Log("[StickmanGrid] Starting initialization...");

            ClearGrid();
            Setup(_levelDataSO.Array2DGrid);

            StartCoroutine(DelayedStickmanSetup());

            _isInitialized = true;
        }

        /// <summary>
        /// Delays the Stickman initialization to ensure proper setup.
        /// </summary>
        private IEnumerator DelayedStickmanSetup()
        {
            yield return new WaitForSeconds(0.1f);

            var allStickmen = GetComponentsInChildren<Stickman>();
            foreach (var stickman in allStickmen)
            {
                if (stickman != null)
                {
                    stickman.Initialize();
                }
            }

            Debug.Log($"[StickmanGrid] Initialized {allStickmen.Length} stickmen");
        }

        /// <summary>
        /// Validates the necessary setup for grid initialization.
        /// </summary>
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

        /// <summary>
        /// Sets up the grid by populating it with Stickman units.
        /// </summary>
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

        /// <summary>
        /// Creates a Stickman at a specific position if needed.
        /// </summary>
        private void CreateStickmanIfNeeded(int x, int y, ColorType colorType)
        {
            if (colorType == ColorType._0None) return;

            Vector3 position = CalculateWorldPosition(x, y);
            Stickman stickman = InstantiateStickman(position, x, y, colorType);

            _stickmanGrid[x, y] = stickman;
            OnStickmanPlaced?.Invoke(new Vector2Int(x, y), stickman);
        }

        /// <summary>
        /// Calculates the world position of a grid cell.
        /// </summary>
        private Vector3 CalculateWorldPosition(int x, int y)
        {
            return new Vector3(x, 0, y) + _gridOffset;
        }

        /// <summary>
        /// Instantiates a Stickman at a given position.
        /// </summary>
        private Stickman InstantiateStickman(Vector3 position, int x, int y, ColorType colorType)
        {
            Stickman stickman = Instantiate(_stickmanPrefab, position, Quaternion.identity);
            stickman.transform.SetParent(transform, worldPositionStays: false);
            stickman.UnitColorType = colorType;
            stickman.SetGridPosition(x, y);

            Tile tile = _tileGrid.GetTileAt(x, y);
            if (tile != null)
            {
                tile.AssignStickman(stickman);
            }

            stickman.Initialize();
            stickman.name = $"Stickman [{x},{y}]";
            return stickman;
        }

        /// <summary>
        /// Clears the entire grid, removing all Stickman units.
        /// </summary>
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

        /// <summary>
        /// Returns the Stickman at a specific grid position.
        /// </summary>
        public Stickman GetStickmanAt(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return _stickmanGrid[x, y];
        }

        /// <summary>
        /// Validates if the position is within the grid's bounds.
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _gridSize.x && y >= 0 && y < _gridSize.y;
        }

        #endregion
    }
}
