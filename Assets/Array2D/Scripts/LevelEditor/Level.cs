using _Main;
using _Main._Stickman.StickmanGrid;
using DG.Tweening;
using LevelEditor;
using SerapKeremGameTools._Game._Singleton;
using System.Collections;
using UnityEngine;

public class Level : MonoSingleton<Level>
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

    #endregion
    #region Events
    public static event System.Action OnLevelCompleted;
    public static event System.Action OnLevelFailed;
    #endregion
    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        FindReferences();
        SubscribeToEvents();
    }
    private void SubscribeToEvents()
    {
        if (tankManager != null)
        {
            Debug.Log("[Level] Subscribing to TankManager events");
            tankManager.OnAllTanksLeft += OnTankManagerCompleted;
        }
    }
    private void UnsubscribeFromEvents()
    {
        if (tankManager != null)
        {
            tankManager.OnAllTanksLeft -= OnTankManagerCompleted;
        }
    }
    private void OnTankManagerCompleted()
    {
        Debug.Log("[Level] All tanks completed, triggering level complete!");
        CompleteLevel();
    }

    private void Start()
    {
        if (!_isInitialized)
        {
            InitializeComponents();
        }
    }
    #endregion

    #region Initialization
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

    private IEnumerator DelayedInitialization()
    {
        yield return null;

        FindReferences(); 
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (!ValidateReferences()) return;

      //  Debug.Log("Initializing Level Components...");

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
    }
    #endregion

    #region Validation
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
    public void CompleteLevel()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[Level] Trying to complete uninitialized level!");
            return;
        }

        Debug.Log("[Level] Level completed! Triggering OnLevelCompleted event");
        OnLevelCompleted?.Invoke();

        // Level animasyonu
        transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .SetEase(Ease.OutBounce);
    }

    public void FailLevel()
    {
        if (!_isInitialized) return;

        Debug.Log("[Level] Level failed! Triggering OnLevelFailed event");
        OnLevelFailed?.Invoke();

        // Level animasyonu
        transform.DOScale(Vector3.one * 0.9f, 0.5f)
            .SetEase(Ease.InBounce);
    }
    #endregion
}