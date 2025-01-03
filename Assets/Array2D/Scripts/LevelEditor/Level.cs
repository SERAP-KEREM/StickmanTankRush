using UnityEngine;
using _Main._Stickman.StickmanGrid;
using LevelEditor;
using _Main;

public class Level : MonoBehaviour
{
    #region Field References

    [SerializeField, Tooltip("Level data containing information for this level.")]
    private LevelDataSO _levelDataSO;

    #endregion
    public static Level Instance { get; private set; }

    [SerializeField] private TileGrid tileGrid;
    [SerializeField] private TankManager tankManager;
    [SerializeField] private StickmanGrid stickmanGrid;
    public TileGrid TileGrid => tileGrid;
    public TankManager TankManager => tankManager;
    public StickmanGrid StickmanGrid => stickmanGrid;

    #region Unity Lifecycle

    private void Awake()
    {
        Instance = this;

        if (tileGrid == null) tileGrid = GetComponentInChildren<TileGrid>();
        if (tankManager == null) tankManager = GetComponentInChildren<TankManager>();
        if (stickmanGrid == null) stickmanGrid = GetComponentInChildren<StickmanGrid>();

        // LevelDataSO'yu tüm gerekli componentlere set et
        if (_levelDataSO != null)
        {
            tileGrid?.SetLevelDataSO(_levelDataSO);
            tankManager?.SetLevelDataSO(_levelDataSO);
            stickmanGrid?.SetLevelDataSO(_levelDataSO);
        }
        else
        {
            Debug.LogError("LevelDataSO is not assigned in Level prefab!");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCreated(this);
        }
    }

    private void Start()
    {
        ValidateLevelReferences();
        TankManager.Instance.SetLevelDataSO(_levelDataSO);
        StickmanGrid.Instance.SetLevelDataSO(_levelDataSO);
        TileGrid.Instance.SetLevelDataSO(_levelDataSO);
 
   
    }

    #endregion

    #region Level Management Methods

    /// <summary>
    /// Initializes the level with specific data.
    /// </summary>
    public void InitializeLevel(LevelDataSO data)
    {
        _levelDataSO = data;
        if (data != null)
        {
            tileGrid?.SetLevelDataSO(data);
            tankManager?.SetLevelDataSO(data);
            stickmanGrid?.SetLevelDataSO(data);
        }
    }

    /// <summary>
    /// Completes the current level and triggers win state.
    /// </summary>
    public void CompleteLevel()
    {
        Debug.Log("Level completed!");
        // TODO: Trigger any win-related events or animations
    }

    /// <summary>
    /// Fails the current level and triggers fail state.
    /// </summary>
    public void FailLevel()
    {
        Debug.Log("Level failed!");
        // TODO: Trigger any fail-related events or animations
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates if the references for the level are correctly set in the inspector.
    /// </summary>
    private void ValidateLevelReferences()
    {
        if (StickmanGrid.Instance == null || _levelDataSO == null)
        {
            Debug.LogError("Level is missing StickmanGrid or LevelDataSO references.");
        }
    }

    #endregion
}
