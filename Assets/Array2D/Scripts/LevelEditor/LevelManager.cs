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

    [SerializeField] private Level _currentLevel;
    private bool _isLevelInProgress;
    private int _currentLevelIndex;
    #endregion

    #region Events
    public static event System.Action OnLevelStarted;
    public static event System.Action OnLevelWon;
    public static event System.Action OnLevelLost;
    #endregion
    [Header("UI References")]
    [SerializeField] private GameplayUI _gameplayUI;
    [SerializeField] private LevelCompleteUI _levelCompleteUI;
    [SerializeField] private LevelFailedUI levelFailedUI;
    private bool isTransitioning = false;

    #region Level Management

    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        if (_levelPrefabs != null && _levelPrefabs.Count > 0)
        {
            _currentLevelIndex = 0;
            StartLevel(_currentLevelIndex);
        }
        else
        {
            Debug.LogError("No level prefabs assigned to LevelManager!");
        }
    }
    private void StartLevel(int levelIndex)
    {
        if (!ValidateLevelIndex(levelIndex))
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }
        ScoreManager.Instance.ResetScores();
        StopAllCoroutines(); 
        StartCoroutine(LoadLevelSequence(levelIndex));
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
    private IEnumerator LoadLevelSequence(int levelIndex)
    {
        UnsubscribeFromEvents();
        if (_currentLevel != null)
        {
            Destroy(_currentLevel.gameObject);
            _currentLevel = null;
            yield return new WaitForSeconds(0.1f); 
        }


        //Debug.Log($"Loading Level {levelIndex}: {levelPrefab.name} with data {levelData.name}");
        if (_gameplayUI != null)
        {
            _gameplayUI.UpdateLevelText(levelIndex + 1); // Level numaras?n? güncelle
            _gameplayUI.ResetProgress();
            _gameplayUI.Show();
        }

        Level levelPrefab = _levelPrefabs[levelIndex];
        LevelDataSO levelData = _levelDataList[levelIndex];

        _currentLevel = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);
        _currentLevel.transform.SetParent(transform, worldPositionStays: true);
        yield return null;


        _currentLevel.InitializeLevel(levelData);

        yield return null;

        if (!ValidateLevelComponents())
        {
            Debug.LogError($"Level {levelIndex} failed to initialize components properly!");
            yield break;
        }
        SubscribeToEvents();
        _currentLevel.transform.localScale = Vector3.zero;
        _currentLevel.transform.DOScale(Vector3.one, _transitionDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                _isLevelInProgress = true;
                OnLevelStarted?.Invoke();
                //Debug.Log($"Level {levelIndex} started successfully");
            });
    }
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
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
            Debug.LogError($"Level component validation failed:\n{errors}");
        }

        return isValid;
    }

    private bool ValidateLevelIndex(int levelIndex)
    {
        if (_levelPrefabs == null || _levelPrefabs.Count == 0)
        {
            Debug.LogError("No level prefabs assigned!");
            return false;
        }

        if (_levelDataList == null || _levelDataList.Count == 0)
        {
            Debug.LogError("No level data assigned!");
            return false;
        }

        if (levelIndex < 0 || levelIndex >= _levelPrefabs.Count)
        {
            Debug.LogError($"Level index {levelIndex} is out of range!");
            return false;
        }

        if (levelIndex >= _levelDataList.Count)
        {
            Debug.LogError($"No level data for index {levelIndex}!");
            return false;
        }

        if (_levelPrefabs[levelIndex] == null)
        {
            Debug.LogError($"Level prefab at index {levelIndex} is null!");
            return false;
        }

        return true;
    }
    #endregion
    #region Win/Lose System
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

    private void HandleLevelWon()
    {

        if (!_isLevelInProgress || isTransitioning) return;

        _isLevelInProgress = false;
        isTransitioning = true;
        ScoreData scoreData = ScoreManager.Instance.GetScoreData();
        Debug.Log($"[LevelManager] Score calculated: {scoreData.TotalScore}");

        // UI kontrolü
        if (_levelCompleteUI == null)
        {
            Debug.LogError("[LevelManager] levelCompleteUI is null!");
            return;
        }
        // Level animasyonu
        _currentLevel.transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                // Score hesapla ve UI'? göster
                ScoreData scoreData = ScoreManager.Instance.GetScoreData();
                _levelCompleteUI.Show(scoreData);
            });
        // Ses efekti
        AudioManager.Instance?.PlayAudio(AudioKeys.LEVEL_WIN);

    }

private void HandleLevelLost()
    {
        if (!_isLevelInProgress) return;

        _isLevelInProgress = false;
        isTransitioning = true;

        _gameplayUI?.Hide();
        // UI kontrolü
        if (levelFailedUI != null)
        {
            Debug.Log("[LevelManager] Showing LevelFailedUI");
            levelFailedUI.gameObject.SetActive(true);
            levelFailedUI.Show();
        }
        Debug.Log("=== LEVEL LOST! ===");
        Debug.Log("All holders are full! Press R to restart.");
        AudioManager.Instance.PlayAudio(AudioKeys.LEVEL_LOSE);
        _currentLevel.transform.DOScale(Vector3.one * 0.9f, 0.5f)
            .SetEase(Ease.InBounce)
            .OnComplete(() => {
                OnLevelLost?.Invoke();
            });
    }
   
    private IEnumerator LoadNextLevelWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextLevel();
    }
    #endregion
    #region Public Methods
    public void LoadNextLevel()
    {
        if (isTransitioning)
        {
            Debug.Log("[LevelManager] Already transitioning to next level");
            return;
        }

        int nextLevelIndex = _currentLevelIndex + 1;
        if (nextLevelIndex >= _levelPrefabs.Count)
        {
            nextLevelIndex = 0;
        }

        StartLevel(nextLevelIndex);
        isTransitioning = false;
    }
    public void OnNextLevelButtonClicked()
    {
        if (!isTransitioning) return;

        isTransitioning = false;
        LoadNextLevel();
    }
    public void RestartLevel()
    {
        StartLevel(_currentLevelIndex);
    }

    public void LoadSpecificLevel(int index)
    {
        if (ValidateLevelIndex(index))
        {
            _currentLevelIndex = index;
            StartLevel(_currentLevelIndex);
        }
    }
    #endregion

    #region Debug
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) 
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