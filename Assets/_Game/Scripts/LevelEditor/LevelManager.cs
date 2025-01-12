using DG.Tweening;
using LevelEditor;
using SerapKeremGameTools._Game._AudioSystem;
using SerapKeremGameTools._Game._Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoSingleton<LevelManager>
{
    #region Fields

    [Header("Level Settings")]
    [SerializeField, Tooltip("List of level prefabs to be instantiated.")]
    private List<Level> _levelPrefabs;

    [SerializeField, Tooltip("List of level data scriptable objects.")]
    private List<LevelDataSO> _levelDataList;

    [SerializeField, Tooltip("Duration of transition animations."), Range(0.1f, 5f)]
    private float _transitionDuration = 1f;

    [Header("UI References")]
    [SerializeField, Tooltip("Reference to the Gameplay UI component.")]
    private GameplayUI _gameplayUI;

    [SerializeField, Tooltip("Reference to the Level Complete UI component.")]
    private LevelCompleteUI _levelCompleteUI;

    [SerializeField, Tooltip("Reference to the Level Failed UI component.")]
    private LevelFailedUI _levelFailedUI;

    private Level _currentLevel;
    private bool _isLevelInProgress;
    private bool _isTransitioning;
    private int _currentLevelIndex;

    /// <summary>
    /// Gets or sets the current level instance.
    /// </summary>
    public Level CurrentLevel
    {
        get => _currentLevel;
        private set => _currentLevel = value;
    }
    #endregion

    #region Events

    public static event System.Action OnLevelStarted;
    public static event System.Action OnLevelWon;
    public static event System.Action OnLevelLost;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Initializes the LevelManager and validates references.
    /// </summary>
     protected override void Awake()
    {
        base.Awake();
        ValidateReferences();
    }

    /// <summary>
    /// Starts the first level on game start.
    /// </summary>
    private void Start()
    {
        InitializeFirstLevel();
    }

    /// <summary>
    /// Subscribes to level completion and failure events.
    /// </summary>
    private void OnEnable()
    {
        Level.OnLevelCompleted += HandleLevelWon;
        Level.OnLevelFailed += HandleLevelLost;
    }

    /// <summary>
    /// Unsubscribes from level completion and failure events.
    /// </summary>
    private void OnDisable()
    {
        Level.OnLevelCompleted -= HandleLevelWon;
        Level.OnLevelFailed -= HandleLevelLost;
    }

    /// <summary>
    /// Cleans up resources and kills all DOTween animations on destroy.
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        DOTween.KillAll();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Validates critical references and logs errors if any are missing.
    /// </summary>
    private void ValidateReferences()
    {
        if (_gameplayUI == null) Debug.LogError("[LevelManager] GameplayUI reference is missing!");
        if (_levelCompleteUI == null) Debug.LogError("[LevelManager] LevelCompleteUI reference is missing!");
        if (_levelFailedUI == null) Debug.LogError("[LevelManager] LevelFailedUI reference is missing!");
    }

    /// <summary>
    /// Initializes the first level in the level list.
    /// </summary>
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

    #endregion

    #region Level Management

    /// <summary>
    /// Starts the level with the specified index.
    /// </summary>
    /// <param name="levelIndex">The index of the level to start.</param>
    private void StartLevel(int levelIndex)
    {
        if (!ValidateLevelIndex(levelIndex))
        {
            Debug.LogError($"[LevelManager] Invalid level index: {levelIndex}");
            return;
        }

        _isLevelInProgress = true;
        _isTransitioning = false;

        ScoreManager.Instance.ResetScores();
        StopAllCoroutines();
        StartCoroutine(LoadLevelSequence(levelIndex));
    }

    /// <summary>
    /// Loads the level sequence for the specified level index.
    /// </summary>
    /// <param name="levelIndex">The index of the level to load.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    private IEnumerator LoadLevelSequence(int levelIndex)
    {
        Debug.Log($"[LevelManager] Starting level sequence for level {levelIndex}");

        UnsubscribeFromEvents();
        yield return CleanupCurrentLevel();
        yield return CreateNewLevel(levelIndex);
        yield return InitializeNewLevel(levelIndex);

        // Animate level entry
        _currentLevel.transform.localScale = Vector3.zero;
        _currentLevel.transform.DOScale(Vector3.one, _transitionDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                _isLevelInProgress = true;
                OnLevelStarted?.Invoke();
                Debug.Log($"[LevelManager] Level {levelIndex} started successfully");
            });
    }

    /// <summary>
    /// Cleans up the current level instance.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    private IEnumerator CleanupCurrentLevel()
    {
        if (_currentLevel != null)
        {
            Destroy(_currentLevel.gameObject);
            _currentLevel = null;
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Instantiates a new level prefab for the specified level index.
    /// </summary>
    /// <param name="levelIndex">The index of the level to create.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    private IEnumerator CreateNewLevel(int levelIndex)
    {
        Level levelPrefab = _levelPrefabs[levelIndex];
        _currentLevel = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);
        _currentLevel.transform.SetParent(transform, worldPositionStays: true);
        yield return null;
    }

    /// <summary>
    /// Initializes the new level with the specified level data.
    /// </summary>
    /// <param name="levelIndex">The index of the level to initialize.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    private IEnumerator InitializeNewLevel(int levelIndex)
    {
        LevelDataSO levelData = _levelDataList[levelIndex];
        _currentLevel.InitializeLevel(levelData);

        if (_gameplayUI != null)
        {
            _gameplayUI.UpdateLevelText(levelIndex + 1);
            _gameplayUI.ResetProgress();
            _gameplayUI.Show();
        }

        yield return null;

        if (!ValidateLevelComponents())
        {
            Debug.LogError($"[LevelManager] Level {levelIndex} failed to initialize!");
            yield break;
        }

        SubscribeToEvents();
    }

    #endregion

    #region Win/Lose Handling

    /// <summary>
    /// Handles the level won event.
    /// </summary>
    private void HandleLevelWon()
    {
        if (!_isLevelInProgress || _isTransitioning)
        {
            Debug.Log("[LevelManager] Ignoring level won - not in progress or transitioning");
            return;
        }

        Debug.Log("[LevelManager] Level Won!");
        _isLevelInProgress = false;
        _isTransitioning = true;

        // Score calculation
        ScoreData scoreData = ScoreManager.Instance.GetScoreData();
        Debug.Log($"[LevelManager] Final Score: {scoreData.TotalScore}");

        // Level animation
        _currentLevel.transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                ShowLevelCompleteUI(scoreData);
            });

        AudioManager.Instance?.PlayAudio(AudioKeys.LEVEL_WIN);
        OnLevelWon?.Invoke();
    }

    /// <summary>
    /// Displays the level complete UI with the provided score data.
    /// </summary>
    /// <param name="scoreData">The score data to display.</param>
    private void ShowLevelCompleteUI(ScoreData scoreData)
    {
        if (_levelCompleteUI != null)
        {
            // Ensure UI is active
            Canvas parentCanvas = _levelCompleteUI.GetComponentInParent<Canvas>();
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
            {
                parentCanvas.gameObject.SetActive(true);
            }

            _levelCompleteUI.gameObject.SetActive(true);
            _levelCompleteUI.Show(scoreData);
            Debug.Log("[LevelManager] Level complete UI shown");
        }
        else
        {
            Debug.LogError("[LevelManager] LevelCompleteUI reference is missing!");
        }
    }

    /// <summary>
    /// Handles the level lost event.
    /// </summary>
    private void HandleLevelLost()
    {
        if (!_isLevelInProgress)
        {
            Debug.Log("[LevelManager] Ignoring level lost - not in progress");
            return;
        }

        Debug.Log("[LevelManager] Level Lost!");
        _isLevelInProgress = false;
        _isTransitioning = true;

        _gameplayUI?.Hide();

        if (_levelFailedUI != null)
        {
            _levelFailedUI.gameObject.SetActive(true);
            _levelFailedUI.Show();
        }

        _currentLevel.transform.DOScale(Vector3.one * 0.9f, 0.5f)
            .SetEase(Ease.InBounce)
            .OnComplete(() => {
                OnLevelLost?.Invoke();
            });

        AudioManager.Instance?.PlayAudio(AudioKeys.LEVEL_LOSE);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Loads the next level in the level list.
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
    /// Handles the next level button click event.
    /// </summary>
    public void OnNextLevelButtonClicked()
    {
        if (!_isTransitioning) return;
        _isTransitioning = false;
        LoadNextLevel();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Subscribes to level-specific events.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (_currentLevel != null)
        {
            _currentLevel.TankManager.OnAllTanksLeft += HandleLevelWon;
            _currentLevel.HolderManager.OnAllHoldersFull += HandleLevelLost;
        }
    }

    /// <summary>
    /// Unsubscribes from level-specific events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (_currentLevel != null)
        {
            _currentLevel.TankManager.OnAllTanksLeft -= HandleLevelWon;
            _currentLevel.HolderManager.OnAllHoldersFull -= HandleLevelLost;
        }
    }

    /// <summary>
    /// Validates the level index to ensure it is within bounds.
    /// </summary>
    /// <param name="levelIndex">The index to validate.</param>
    /// <returns>True if the index is valid, otherwise false.</returns>
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
            Debug.LogError($"[LevelManager] Level index {levelIndex} is out of range!");
            return false;
        }

        if (levelIndex >= _levelDataList.Count)
        {
            Debug.LogError($"[LevelManager] No level data for index {levelIndex}!");
            return false;
        }

        if (_levelPrefabs[levelIndex] == null)
        {
            Debug.LogError($"[LevelManager] Level prefab at index {levelIndex} is null!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates the components of the current level.
    /// </summary>
    /// <returns>True if all components are valid, otherwise false.</returns>
    private bool ValidateLevelComponents()
    {
        if (_currentLevel == null) return false;

        bool isValid = true;
        string errors = "";

        if (_currentLevel.TileGrid == null)
        {
            errors += "TileGrid is null\n";
            isValid = false;
        }

        if (_currentLevel.TankManager == null)
        {
            errors += "TankManager is null\n";
            isValid = false;
        }

        if (_currentLevel.StickmanGrid == null)
        {
            errors += "StickmanGrid is null\n";
            isValid = false;
        }

        if (!isValid)
        {
            Debug.LogError($"[LevelManager] Level component validation failed:\n{errors}");
        }

        return isValid;
    }

    #endregion

    #region Debug

    /// <summary>
    /// Handles debug input for testing purposes.
    /// </summary>
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
#endif
    }

    #endregion
}