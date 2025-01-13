using _Main;
using _Main._Stickman.PathSystem;
using _Main._Stickman.StickmanGrid;
using DG.Tweening;
using LevelEditor;
using SerapKeremGameTools._Game._Singleton;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class Level : MonoBehaviour
{
    #region Fields

    [Header("Level Configuration")]
    [SerializeField] private LevelDataSO _levelDataSO;

    [Header("Component References")]
    [SerializeField] private TileGrid tileGrid;
    [SerializeField] private TankManager tankManager;
    [SerializeField] private StickmanGrid stickmanGrid;
    [SerializeField] private HolderManager holderManager;

    public TileGrid TileGrid => tileGrid;
    public TankManager TankManager => tankManager;
    public StickmanGrid StickmanGrid => stickmanGrid;
    public HolderManager HolderManager => holderManager;

    private bool _isInitialized = false;
    [Header("Path System")]
    [SerializeField] private PathFinder _pathFinder;
    [SerializeField] private NavMeshSurface _navMeshSurface;
    #endregion

    #region Events
    public static event System.Action OnLevelCompleted;
    public static event System.Action OnLevelFailed;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Called when the script instance is being loaded.
    /// Finds references for necessary components and subscribes to relevant events.
    /// </summary>
    private void Awake()
    {
        FindReferences();
        SubscribeToEvents();
    }

    /// <summary>
    /// Subscribes to events from the TankManager.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (tankManager != null)
        {
            Debug.Log("[Level] Subscribing to TankManager events");
            tankManager.OnAllTanksLeft += OnTankManagerCompleted;
        }
    }

    /// <summary>
    /// Unsubscribes from events when no longer needed.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (tankManager != null)
        {
            tankManager.OnAllTanksLeft -= OnTankManagerCompleted;
        }
    }

    /// <summary>
    /// Callback when all tanks have completed their tasks, triggering the level to complete.
    /// </summary>
    private void OnTankManagerCompleted()
    {
        Debug.Log("[Level] All tanks completed, triggering level complete!");
        CompleteLevel();
    }

    /// <summary>
    /// Called when the object is initialized. Ensures components are initialized if not already.
    /// </summary>
    private void Start()
    {
        if (!_isInitialized)
        {
            InitializeComponents();
        }
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Finds references for all required components (TileGrid, TankManager, StickmanGrid, HolderManager).
    /// </summary>
    private void FindReferences()
    {
        if (tileGrid == null)
        {
            tileGrid = GetComponentInChildren<TileGrid>(true);
            Debug.Log($"Found TileGrid: {tileGrid != null}");
        }

        if (tankManager == null)
        {
            tankManager = GetComponentInChildren<TankManager>(true);
            Debug.Log($"Found TankManager: {tankManager != null}");
        }

        if (stickmanGrid == null)
        {
            stickmanGrid = GetComponentInChildren<StickmanGrid>(true);
            Debug.Log($"Found StickmanGrid: {stickmanGrid != null}");
        }

        if (holderManager == null)
        {
            holderManager = GetComponentInChildren<HolderManager>(true);
            Debug.Log($"Found StickmanGrid: {holderManager != null}");
        }
    }

    /// <summary>
    /// Initializes the level with the provided LevelDataSO object.
    /// </summary>
    /// <param name="data">Level data to initialize the level with.</param>
    public void InitializeLevel(LevelDataSO data)
    {
        if (data == null)
        {
            Debug.LogError("Trying to initialize level with null data!");
            return;
        }

        _levelDataSO = data;
        _isInitialized = false; // Reset initialization flag

        StartCoroutine(DelayedInitialization());
    }

    /// <summary>
    /// Delays the initialization process to allow references to be found first.
    /// </summary>
    /// <returns>Enumerator for coroutine.</returns>
    private IEnumerator DelayedInitialization()
    {
        yield return null;

        FindReferences();
        InitializeComponents();
    }

    /// <summary>
    /// Initializes the level components (StickmanGrid, TileGrid, TankManager, HolderManager).
    /// </summary>
    private void InitializeComponents()
    {
        if (!ValidateReferences()) return;

        // Initialize components
        if (stickmanGrid != null)
        {
            stickmanGrid.SetLevelDataSO(_levelDataSO);
            //Debug.Log("StickmanGrid initialized");
        }

        if (tileGrid != null)
        {
            tileGrid.SetLevelDataSO(_levelDataSO);
            //Debug.Log("TileGrid initialized");
        }

        if (tankManager != null)
        {
            tankManager.SetLevelDataSO(_levelDataSO);
            // Debug.Log("TankManager initialized");
        }

        if (holderManager != null)
        {
            holderManager.InitializeWaitingRow();
            // Debug.Log("TankManager initialized");
        }

        _isInitialized = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCreated(this);
        }

        if (_pathFinder == null)
        {
            var pathFinderObj = new GameObject("PathFinder");
            _pathFinder = pathFinderObj.AddComponent<PathFinder>();
            pathFinderObj.transform.SetParent(transform);
        }

        if (_navMeshSurface != null)
        {
            _navMeshSurface.BuildNavMesh();
        }
    }
    #endregion

    #region Validation
    /// <summary>
    /// Validates that all necessary references are set and not null.
    /// </summary>
    /// <returns>True if all references are valid, otherwise false.</returns>
    private bool ValidateReferences()
    {
        bool isValid = true;

        if (_levelDataSO == null)
        {
            Debug.LogError("LevelDataSO is missing!");
            isValid = false;
        }

        if (tileGrid == null)
        {
            Debug.LogError("TileGrid reference is missing!");
            isValid = false;
        }

        if (tankManager == null)
        {
            Debug.LogError("TankManager reference is missing!");
            isValid = false;
        }

        if (stickmanGrid == null)
        {
            Debug.LogError("StickmanGrid reference is missing!");
            isValid = false;
        }

        if (holderManager == null)
        {
            Debug.LogError("HolderManager reference is missing!");
            isValid = false;
        }

        return isValid;
    }
    #endregion

    #region Level State
    /// <summary>
    /// Completes the current level and triggers the level completion event.
    /// </summary>
    public void CompleteLevel()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[Level] Trying to complete uninitialized level!");
            return;
        }

        Debug.Log("[Level] Level completed! Triggering OnLevelCompleted event");
        OnLevelCompleted?.Invoke();

        // Level animation
        transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .SetEase(Ease.OutBounce);
    }

    /// <summary>
    /// Fails the current level and triggers the level failure event.
    /// </summary>
    public void FailLevel()
    {
        if (!_isInitialized) return;

        Debug.Log("[Level] Level failed! Triggering OnLevelFailed event");
        OnLevelFailed?.Invoke();

        // Level animation
        transform.DOScale(Vector3.one * 0.9f, 0.5f)
            .SetEase(Ease.InBounce);
    }
    #endregion
}
