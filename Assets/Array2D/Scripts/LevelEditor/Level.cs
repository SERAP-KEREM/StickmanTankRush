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
        // Child componentleri bul
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

        // Bir frame bekleyip sonra initialize et
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        // Awake ça?r?lar?n?n tamamlanmas? için bekle
        yield return null;

        FindReferences(); // Referanslar? tekrar kontrol et
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (!ValidateReferences()) return;

        Debug.Log("Initializing Level Components...");

        // StickmanGrid'i initialize et
        if (stickmanGrid != null)
        {
            stickmanGrid.SetLevelDataSO(_levelDataSO);
            stickmanGrid.Initialize();
            Debug.Log("StickmanGrid initialized");
        }

        // TileGrid'i initialize et
        if (tileGrid != null)
        {
            tileGrid.SetLevelDataSO(_levelDataSO);
            tileGrid.Initialize();
            Debug.Log("TileGrid initialized");
        }

        // TankManager'? initialize et
        if (tankManager != null)
        {
            tankManager.SetLevelDataSO(_levelDataSO);
            //tankManager.Initialize();
            Debug.Log("TankManager initialized");
        } 
        if (holderManager != null)
        {

            holderManager.InitializeWaitingRow();
            Debug.Log("TankManager initialized");
        }

        _isInitialized = true;

        // GameManager'a haber ver
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