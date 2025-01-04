using DG.Tweening;
using LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    #region Fields
    [Header("Level Settings")]
    [SerializeField] private List<Level> _levelPrefabs;
    [SerializeField] private List<LevelDataSO> _levelDataList;
    [SerializeField] private float _transitionDuration = 1f;

    [SerializeField] private Level _currentLevel;
    private bool _isLevelInProgress;
    private int _currentLevelIndex;

    // Events
    public static event System.Action OnLevelStarted;
    #endregion

    #region Level Management
    private void Start()
    {
        // Oyun ba?lad???nda ilk leveli yükle
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

        StopAllCoroutines(); // Önceki tüm coroutine'leri durdur
        StartCoroutine(LoadLevelSequence(levelIndex));
    }

    private IEnumerator LoadLevelSequence(int levelIndex)
    {
        // 1. Önceki leveli temizle
        if (_currentLevel != null)
        {
            Destroy(_currentLevel.gameObject);
            _currentLevel = null;
            yield return new WaitForSeconds(0.1f); // Destruction için k?sa bir bekleme
        }

        // 2. Yeni level için haz?rl?k
        Level levelPrefab = _levelPrefabs[levelIndex];
        LevelDataSO levelData = _levelDataList[levelIndex];

        Debug.Log($"Loading Level {levelIndex}: {levelPrefab.name} with data {levelData.name}");

        // 3. Yeni leveli olu?tur
        _currentLevel = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);
        _currentLevel.transform.SetParent(transform, worldPositionStays: true);

        // 4. ?lk frame'i bekle
        yield return null;

        // 5. Level'? initialize et
        _currentLevel.InitializeLevel(levelData);

        // 6. Bir frame daha bekle
        yield return null;

        // 7. Component'leri kontrol et
        if (!ValidateLevelComponents())
        {
            Debug.LogError($"Level {levelIndex} failed to initialize components properly!");
            yield break;
        }

        // 8. Level ba?lang?ç animasyonu
        _currentLevel.transform.localScale = Vector3.zero;
        _currentLevel.transform.DOScale(Vector3.one, _transitionDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                _isLevelInProgress = true;
                OnLevelStarted?.Invoke();
                Debug.Log($"Level {levelIndex} started successfully");
            });
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
        // Test için
        if (Input.GetKeyDown(KeyCode.N)) // Next Level
        {
            LoadNextLevel();
        }
        if (Input.GetKeyDown(KeyCode.R)) // Restart
        {
            RestartLevel();
        }
    }
    #endregion
}