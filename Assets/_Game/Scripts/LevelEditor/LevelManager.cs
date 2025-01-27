using UnityEngine;
using DG.Tweening;
using LevelEditor;
using SerapKeremGameTools._Game._AudioSystem;
using SerapKeremGameTools._Game._Singleton;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using _Main;

/// <summary>
/// Manages level loading, transitions, and game state.
/// Handles level progression and UI interactions.
/// </summary>
[DeclareFoldoutGroup("Settings", Title = "Level Settings")]
[DeclareFoldoutGroup("UI", Title = "UI References")]
[DeclareFoldoutGroup("State", Title = "Level State")]
public class LevelManager : MonoSingleton<LevelManager>
{
    #region Settings
    [Group("Settings")]
    [SerializeField, Required]
    [PropertyTooltip("List of level prefabs to instantiate")]
    private List<Level> _levelPrefabs;

    [Group("Settings")]
    [SerializeField, Required]
    [PropertyTooltip("List of level configuration data")]
    private List<LevelDataSO> _levelDataList;

    [Group("Settings")]
    [SerializeField]
    [PropertyTooltip("Duration of level transition animations")]
    [Range(0.1f, 5f)]
    private float _transitionDuration = 1f;
    #endregion

    #region UI References
    [Group("UI")]
    [SerializeField, Required]
    [PropertyTooltip("Main gameplay UI reference")]
    private GameplayUI _gameplayUI;

    [Group("UI")]
    [SerializeField, Required]
    [PropertyTooltip("Level complete UI reference")]
    private LevelCompleteUI _levelCompleteUI;

    [Group("UI")]
    [SerializeField, Required]
    [PropertyTooltip("Level failed UI reference")]
    private LevelFailedUI _levelFailedUI;
    #endregion

    #region State
    [Group("State")]
    [SerializeField, ReadOnly]
    private Level _currentLevel;

    [Group("State")]
    [SerializeField, ReadOnly]
    private bool _isLevelInProgress;

    [Group("State")]
    [SerializeField, ReadOnly]
    private bool _isTransitioning;

    [Group("State")]
    [SerializeField, ReadOnly]
    private int _currentLevelIndex;
    #endregion

    #region Events
    public static event System.Action OnLevelStarted;
    public static event System.Action OnLevelWon;
    public static event System.Action OnLevelLost;
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        ValidateReferences();
    }

    private void Start()
    {
        InitializeFirstLevel();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        DOTween.KillAll();
    }

    private void Update()
    {
        HandleDebugInput();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Loads the next level in sequence.
    /// </summary>
    public void LoadNextLevel()
    {
        _currentLevelIndex = (_currentLevelIndex + 1) % _levelPrefabs.Count;
        StartLevel(_currentLevelIndex);
    }

    /// <summary>
    /// Restarts the current level.
    /// </summary>
    public void RestartLevel()
    {
        StartLevel(_currentLevelIndex);
    }

    /// <summary>
    /// Handles the next level button click.
    /// </summary>
    public void OnNextLevelButtonClicked()
    {
        if (!_isTransitioning) return;
        _isTransitioning = false;
        LoadNextLevel();
    }
    #endregion

    #region Level Management
    private void StartLevel(int levelIndex)
    {
        if (!ValidateLevelIndex(levelIndex)) return;

        _isLevelInProgress = true;
        _isTransitioning = false;

        ScoreManager.Instance.ResetScores();
        StopAllCoroutines();
        StartCoroutine(LoadLevelSequence(levelIndex));
    }

    private IEnumerator LoadLevelSequence(int levelIndex)
    {
        Debug.Log($"[LevelManager] Loading level {levelIndex}");

        UnsubscribeFromEvents();
        yield return CleanupCurrentLevel();
        yield return CreateNewLevel(levelIndex);
        yield return InitializeNewLevel(levelIndex);
       
        AnimateLevelEntry();
    }

    private void AnimateLevelEntry()
    {
        _currentLevel.transform.localScale = Vector3.zero;
        _currentLevel.transform.DOScale(Vector3.one, _transitionDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                _isLevelInProgress = true;
                OnLevelStarted?.Invoke();
            });
    }
    #endregion

    #region Level State Handling
    private void HandleLevelWon()
    {
        if (!ValidateLevelState("won")) return;

        _isLevelInProgress = false;
        _isTransitioning = true;

        ScoreData scoreData = ScoreManager.Instance.GetScoreData();
        AnimateLevelWin();
        ShowLevelCompleteUI(scoreData);

        AudioManager.Instance?.PlayAudioByName(AudioKeys.LEVEL_WIN);
        OnLevelWon?.Invoke();
    }

    private void HandleLevelLost()
    {
        if (!ValidateLevelState("lost")) return;

        _isLevelInProgress = false;
        _isTransitioning = true;

        _gameplayUI?.Hide();
        ShowLevelFailedUI();
        AnimateLevelLose();

        OnLevelLost?.Invoke();
    }
    #endregion

    #region UI Management
    private void ShowLevelCompleteUI(ScoreData scoreData)
    {
        if (_levelCompleteUI == null)
        {
            Debug.LogError("[LevelManager] LevelCompleteUI is missing!");
            return;
        }

        EnsureUICanvasActive(_levelCompleteUI);
        _levelCompleteUI.gameObject.SetActive(true);
        _levelCompleteUI.Show(scoreData);
    }

    private void ShowLevelFailedUI()
    {
        if (_levelFailedUI != null)
        {
            _levelFailedUI.gameObject.SetActive(true);
            _levelFailedUI.Show();
        }
    }

    private void EnsureUICanvasActive(MonoBehaviour ui)
    {
        var parentCanvas = ui.GetComponentInParent<Canvas>();
        if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
        {
            parentCanvas.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Animation
    private void AnimateLevelWin()
    {
        _currentLevel.transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .SetEase(Ease.OutBounce);
    }

    private void AnimateLevelLose()
    {
        _currentLevel.transform.DOScale(Vector3.one * 0.9f, 0.5f)
            .SetEase(Ease.InBounce);
    }
    #endregion

    #region Initialization
    private void InitializeFirstLevel()
    {
        if (_levelPrefabs != null && _levelPrefabs.Count > 0)
        {
            _currentLevelIndex = 0;
            StartLevel(_currentLevelIndex);
        }
        else
        {
            Debug.LogError("[LevelManager] No level prefabs assigned!");
        }
    }

    private IEnumerator CleanupCurrentLevel()
    {
        if (_currentLevel != null)
        {
            Destroy(_currentLevel.gameObject);
            _currentLevel = null;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator CreateNewLevel(int levelIndex)
    {
        Level levelPrefab = _levelPrefabs[levelIndex];
        _currentLevel = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);
        _currentLevel.transform.SetParent(transform, worldPositionStays: true);
        yield return null;
        
    }

    private IEnumerator InitializeNewLevel(int levelIndex)
    {
        LevelDataSO levelData = _levelDataList[levelIndex];
        _currentLevel.InitializeLevel(levelData);

        InitializeUI(levelIndex);
        yield return null;

        if (!ValidateLevelComponents())
        {
            Debug.LogError($"[LevelManager] Level {levelIndex} failed to initialize!");
            yield break;
        }

        SubscribeToEvents();
    }

    private void InitializeUI(int levelIndex)
    {
        if (_gameplayUI != null)
        {
            _gameplayUI.UpdateLevelText(levelIndex + 1);
            _gameplayUI.ResetProgress();
            _gameplayUI.Show();
        }
    }
    #endregion

    #region Event Management
    private void SubscribeToEvents()
    {
        Level.OnLevelCompleted += HandleLevelWon;
        Level.OnLevelFailed += HandleLevelLost;

        if (_currentLevel != null)
        {
            _currentLevel.TankManager.OnAllTanksLeft += HandleLevelWon;
            _currentLevel.HolderManager.OnAllHoldersFull += HandleLevelLost;
        }
    }

    private void UnsubscribeFromEvents()
    {
        Level.OnLevelCompleted -= HandleLevelWon;
        Level.OnLevelFailed -= HandleLevelLost;

        if (_currentLevel != null)
        {
            _currentLevel.TankManager.OnAllTanksLeft -= HandleLevelWon;
            _currentLevel.HolderManager.OnAllHoldersFull -= HandleLevelLost;
        }
    }
    #endregion

    #region Validation
    private void ValidateReferences()
    {
        if (_gameplayUI == null) Debug.LogError("[LevelManager] GameplayUI is missing!");
        if (_levelCompleteUI == null) Debug.LogError("[LevelManager] LevelCompleteUI is missing!");
        if (_levelFailedUI == null) Debug.LogError("[LevelManager] LevelFailedUI is missing!");
    }

    private bool ValidateLevelIndex(int levelIndex)
    {
        if (_levelPrefabs == null || _levelPrefabs.Count == 0)
        {
            Debug.LogError("[LevelManager] No level prefabs assigned!");
            return false;
        }

        if (_levelDataList == null || _levelDataList.Count == 0)
        {
            Debug.LogError("[LevelManager] No level data assigned!");
            return false;
        }

        if (levelIndex < 0 || levelIndex >= _levelPrefabs.Count)
        {
            Debug.LogError($"[LevelManager] Invalid level index: {levelIndex}");
            return false;
        }

        if (levelIndex >= _levelDataList.Count)
        {
            Debug.LogError($"[LevelManager] No level data for index: {levelIndex}");
            return false;
        }

        return true;
    }
    private bool ValidateLevelState(string action)
    {
        if (!_isLevelInProgress || (_isTransitioning && action == "won"))
        {
            Debug.Log($"[LevelManager] Ignoring level {action} - invalid state");
            return false;
        }
        return true;
    }
    private bool ValidateLevelComponents()
    {
        if (_currentLevel == null) return false;

        bool isValid = true;
        List<string> errors = new List<string>();

        if (_currentLevel.TileGrid == null) errors.Add("TileGrid is null");
        if (_currentLevel.TankManager == null) errors.Add("TankManager is null");
        if (_currentLevel.StickmanGrid == null) errors.Add("StickmanGrid is null");

        if (errors.Count > 0)
        {
            Debug.LogError($"[LevelManager] Level component validation failed:\n{string.Join("\n", errors)}");
            isValid = false;
        }

        return isValid;
    }
    #endregion

    #region Debug
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }
    #endregion
}
