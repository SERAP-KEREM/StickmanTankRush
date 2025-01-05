using _Main;
using _Main._Stickman.StickmanGrid;
using LevelEditor;
using System.Collections;
using UnityEngine;

public class Level : MonoBehaviour
{
    #region Fields
    public static Level Instance { get; private set; }

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

    #region Unity Lifecycle
    private void Awake()
    {
        Instance = this;
        FindReferences();
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

        Debug.Log("Initializing Level Components...");

        if (stickmanGrid != null)
        {
            stickmanGrid.SetLevelDataSO(_levelDataSO);
            Debug.Log("StickmanGrid initialized");
        }

        if (tileGrid != null)
        {
            tileGrid.SetLevelDataSO(_levelDataSO);
            Debug.Log("TileGrid initialized");
        }

        if (tankManager != null)
        {
            tankManager.SetLevelDataSO(_levelDataSO);
            Debug.Log("TankManager initialized");
        } 
        if (holderManager != null)
        {

            holderManager.InitializeWaitingRow();
            Debug.Log("TankManager initialized");
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
        if (!_isInitialized) return;
        Debug.Log("Level completed!");
    }

    public void FailLevel()
    {
        if (!_isInitialized) return;
        Debug.Log("Level failed!");
    }
    #endregion
}