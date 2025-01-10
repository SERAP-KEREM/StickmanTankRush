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
    [SerializeField] private List<Level> _levelPrefabs;
    [SerializeField] private List<LevelDataSO> _levelDataList;
    [SerializeField] private float _transitionDuration = 1f;

    [Header("UI References")]
    [SerializeField] private GameplayUI _gameplayUI;
    [SerializeField] private LevelCompleteUI _levelCompleteUI;
    [SerializeField] private LevelFailedUI levelFailedUI;

    private Level _currentLevel;
    private bool _isLevelInProgress;
    private bool _isTransitioning;
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
        Level.OnLevelCompleted += HandleLevelWon;
        Level.OnLevelFailed += HandleLevelLost;
    }

    private void OnDisable()
    {
        Level.OnLevelCompleted -= HandleLevelWon;
        Level.OnLevelFailed -= HandleLevelLost;
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    #endregion

    #region Initialization
    private void ValidateReferences()
    {
        if (_gameplayUI == null) Debug.LogError("[LevelManager] GameplayUI reference is missing!");
        if (_levelCompleteUI == null) Debug.LogError("[LevelManager] LevelCompleteUI reference is missing!");
        if (levelFailedUI == null) Debug.LogError("[LevelManager] LevelFailedUI reference is missing!");
    }

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

        if (levelFailedUI != null)
        {
            levelFailedUI.gameObject.SetActive(true);
            levelFailedUI.Show();
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
    public void LoadNextLevel()
    {
        _currentLevelIndex = (_currentLevelIndex + 1) % _levelPrefabs.Count;
        StartLevel(_currentLevelIndex);
    }

    public void RestartLevel()
    {
        StartLevel(_currentLevelIndex);
    }

    public void OnNextLevelButtonClicked()
    {
        if (!_isTransitioning) return;
        _isTransitioning = false;
        LoadNextLevel();
    }
    #endregion

    #region Utility Methods
    private void SubscribeToEvents()
    {
        if (_currentLevel != null)
        {
            _currentLevel.TankManager.OnAllTanksLeft += HandleLevelWon;
            _currentLevel.HolderManager.OnAllHoldersFull += HandleLevelLost;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_currentLevel != null)
        {
            _currentLevel.TankManager.OnAllTanksLeft -= HandleLevelWon;
            _currentLevel.HolderManager.OnAllHoldersFull -= HandleLevelLost;
        }
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