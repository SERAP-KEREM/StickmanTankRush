using UnityEngine;
using _Main._Stickman.StickmanGrid;
using LevelEditor;

public class Level : MonoBehaviour
{
    #region Field References

    [Header("References")]
    [SerializeField, Tooltip("Manages all tank operations in the current level.")]
    private TankManager _tankManager;

    [SerializeField, Tooltip("Handles the Stickman grid for the current level.")]
    private StickmanGrid _stickmanGrid;

    [SerializeField, Tooltip("Level data containing information for this level.")]
    private LevelDataSO _levelDataSO;

    #endregion

    #region Properties

    public StickmanGrid StickmanGrid => _stickmanGrid;
    public LevelDataSO LevelDataSO => _levelDataSO;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        ValidateLevelReferences();
        TankManager.Instance.SetLevelDataSO(_levelDataSO);
    }

    #endregion

    #region Level Management Methods

    /// <summary>
    /// Initializes the level with specific data.
    /// </summary>
    public void InitializeLevel(LevelDataSO levelData)
    {
        _levelDataSO = levelData;
        Debug.Log($"Level initialized with {levelData.name}");
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
        if (_stickmanGrid == null || _levelDataSO == null)
        {
            Debug.LogError("Level is missing StickmanGrid or LevelDataSO references.");
        }
    }

    #endregion
}
