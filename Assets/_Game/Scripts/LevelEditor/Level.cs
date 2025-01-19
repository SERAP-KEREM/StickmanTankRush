using UnityEngine;
using _Main._Stickman.StickmanGrid;
using DG.Tweening;
using LevelEditor;
using System.Collections;
using TriInspector;

namespace _Main
{
    /// <summary>
    /// Manages level initialization, component references, and level state.
    /// Coordinates between different managers and handles level completion logic.
    /// </summary>
    [DeclareFoldoutGroup("Configuration", Title = "Level Settings")]
    [DeclareFoldoutGroup("Components", Title = "Component References")]
    public class Level : MonoBehaviour
    {
        #region Configuration
        [Group("Configuration")]
        [SerializeField, Required]
        [PropertyTooltip("Scriptable object containing level configuration data")]
        private LevelDataSO _levelDataSO;

        [Group("Configuration")]
        [SerializeField, ReadOnly]
        private bool _isInitialized;
        #endregion

        #region Component References
        [Group("Components")]
        [SerializeField, Required]
        [PropertyTooltip("Manages the tile grid system")]
        private TileGrid _tileGrid;

        [Group("Components")]
        [SerializeField, Required]
        [PropertyTooltip("Manages tank spawning and movement")]
        private TankManager _tankManager;

        [Group("Components")]
        [SerializeField, Required]
        [PropertyTooltip("Manages stickman grid placement")]
        private StickmanGrid _stickmanGrid;

        [Group("Components")]
        [SerializeField, Required]
        [PropertyTooltip("Manages holder positions and assignments")]
        private HolderManager _holderManager;

        [Group("Components")]
        [SerializeField, Required]
        [PropertyTooltip("Handles pathfinding for stickmen")]
        private GridPathFinder _gridPathFinder;
        #endregion

        #region Properties
        public TileGrid TileGrid => _tileGrid;
        public TankManager TankManager => _tankManager;
        public StickmanGrid StickmanGrid => _stickmanGrid;
        public HolderManager HolderManager => _holderManager;
        public GridPathFinder GridPathFinder => _gridPathFinder;
        #endregion

        #region Events
        public static event System.Action OnLevelCompleted;
        public static event System.Action OnLevelFailed;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            FindReferences();
            SubscribeToEvents();
        }

        private void Start()
        {
            if (!_isInitialized)
            {
                InitializeComponents();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the level with provided level data.
        /// </summary>
        public void InitializeLevel(LevelDataSO data)
        {
            if (!ValidateLevelData(data)) return;

            _levelDataSO = data;
            _isInitialized = false;
            StartCoroutine(DelayedInitialization());
        }

        /// <summary>
        /// Completes the level with success animation.
        /// </summary>
        public void CompleteLevel()
        {
            if (!ValidateLevelState("complete")) return;

            Debug.Log("[Level] Level completed!");
            OnLevelCompleted?.Invoke();
            PlayCompletionAnimation();
        }

        /// <summary>
        /// Fails the level with failure animation.
        /// </summary>
        public void FailLevel()
        {
            if (!ValidateLevelState("fail")) return;

            Debug.Log("[Level] Level failed!");
            OnLevelFailed?.Invoke();
            PlayFailureAnimation();
        }
        #endregion

        #region Initialization
        private void FindReferences()
        {
            _tileGrid ??= GetComponentInChildren<TileGrid>(true);
            _tankManager ??= GetComponentInChildren<TankManager>(true);
            _stickmanGrid ??= GetComponentInChildren<StickmanGrid>(true);
            _holderManager ??= GetComponentInChildren<HolderManager>(true);
            _gridPathFinder ??= GetComponent<GridPathFinder>() ?? gameObject.AddComponent<GridPathFinder>();

            LogComponentStatus();
        }

        private void InitializeComponents()
        {
            if (!ValidateReferences()) return;

            InitializeGrids();
            InitializeManagers();
            InitializePathfinding();

            _isInitialized = true;
            NotifyGameManager();
        }

        private void InitializeGrids()
        {
            _stickmanGrid?.SetLevelDataSO(_levelDataSO);
            _tileGrid?.SetLevelDataSO(_levelDataSO);
        }

        private void InitializeManagers()
        {
            _tankManager?.SetLevelDataSO(_levelDataSO);
            _holderManager?.InitializeWaitingRow(); 
        }

        private void InitializePathfinding()
        {
            if (_gridPathFinder != null && _tileGrid != null)
            {
                _gridPathFinder.Initialize(_tileGrid);
            }
        }
        #endregion

        #region Event Handling
        private void SubscribeToEvents()
        {
            if (_tankManager != null)
            {
                _tankManager.OnAllTanksLeft += OnTankManagerCompleted;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_tankManager != null)
            {
                _tankManager.OnAllTanksLeft -= OnTankManagerCompleted;
            }
        }

        private void OnTankManagerCompleted()
        {
            CompleteLevel();
        }
        #endregion

        #region Validation
        private bool ValidateLevelData(LevelDataSO data)
        {
            if (data == null)
            {
                Debug.LogError("[Level] Cannot initialize with null level data!");
                return false;
            }
            return true;
        }

        private bool ValidateLevelState(string action)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[Level] Cannot {action} uninitialized level!");
                return false;
            }
            return true;
        }

        private bool ValidateReferences()
        {
            if (_levelDataSO == null || _tileGrid == null || _tankManager == null ||
                _stickmanGrid == null || _holderManager == null || _gridPathFinder == null)
            {
                Debug.LogError("[Level] One or more required components are missing!");
                return false;
            }
            return true;
        }
        #endregion

        #region Helper Methods
        private IEnumerator DelayedInitialization()
        {
            yield return null;
            FindReferences();
            InitializeComponents();
        }

        private void NotifyGameManager()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelCreated(this);
            }
        }

        private void PlayCompletionAnimation()
        {
            transform.DOScale(Vector3.one * 1.1f, 0.5f)
                .SetEase(Ease.OutBounce);
        }

        private void PlayFailureAnimation()
        {
            transform.DOScale(Vector3.one * 0.9f, 0.5f)
                .SetEase(Ease.InBounce);
        }

        private void LogComponentStatus()
        {
            Debug.Log($"[Level] Component Status:\n" +
                     $"TileGrid: {_tileGrid != null}\n" +
                     $"TankManager: {_tankManager != null}\n" +
                     $"StickmanGrid: {_stickmanGrid != null}\n" +
                     $"HolderManager: {_holderManager != null}\n" +
                     $"GridPathFinder: {_gridPathFinder != null}");
        }
        #endregion
    }
}