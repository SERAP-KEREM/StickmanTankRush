using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LevelEditor;

namespace _Main
{
    public class LevelManager : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Level Settings")]
        [Tooltip("List of all available level prefabs.")]
        [SerializeField] private List<Level> _levelPrefabs;

        [Header("Game State Variables")]
        [Tooltip("Index of the current level.")]
        [SerializeField] private int _currentLevelIndex = 0;

        [Tooltip("Level transition duration in seconds.")]
        [SerializeField] private float _transitionDuration = 1f;

        [Header("Level Data")]
        [Tooltip("List of level data objects matching the order of level prefabs.")]
        [SerializeField] private List<LevelDataSO> _levelDataList;

        #endregion

        #region Events

        public static event Action OnLevelStarted;
        public static event Action OnLevelCompleted;
        public static event Action OnLevelFailed;
        public static event Action OnLevelPaused;

        #endregion

        #region Private Fields

        private static LevelManager _instance;
        private Level _currentLevel;
        private bool _isLevelInProgress = false;

        #endregion

        #region Singleton Pattern

        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LevelManager>();
                    if (_instance == null)
                    {
                        Debug.LogError("No LevelManager found in the scene.");
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (_levelPrefabs == null || _levelPrefabs.Count == 0)
            {
                Debug.LogError("No level prefabs assigned in the LevelManager.");
                return;
            }

            if (_levelDataList == null || _levelDataList.Count != _levelPrefabs.Count)
            {
                Debug.LogError("Level data list must be assigned and must match the size of level prefabs.");
                return;
            }

            StartLevel(_currentLevelIndex);
        }

        #endregion

        #region Level Management

        /// <summary>
        /// Starts a level by index.
        /// </summary>
        /// <param name="levelIndex">Index of the level to start.</param>
        public void StartLevel(int levelIndex)
        {
            if (!ValidateLevelIndex(levelIndex)) return;

            // Önceki level'? temizle
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
            }

            // Yeni level'? olu?tur
            Level levelPrefab = _levelPrefabs[levelIndex];
            LevelDataSO levelData = _levelDataList[levelIndex];

            _currentLevel = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);
            _currentLevel.InitializeLevel(levelData);
        }

        private bool ValidateLevelIndex(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _levelPrefabs.Count)
            {
                Debug.LogError($"Invalid level prefab index: {levelIndex}");
                return false;
            }

            if (levelIndex >= _levelDataList.Count)
            {
                Debug.LogError($"Missing level data for index: {levelIndex}");
                return false;
            }

            if (_levelPrefabs[levelIndex] == null)
            {
                Debug.LogError($"Level prefab at index {levelIndex} is null!");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Completes the current level and loads the next level.
        /// </summary>
        public void CompleteLevel()
        {
            if (!_isLevelInProgress) return;

            _isLevelInProgress = false;
            _currentLevel.CompleteLevel();
            OnLevelCompleted?.Invoke();

            // Load the next level
            int nextLevelIndex = _currentLevelIndex + 1;
            if (nextLevelIndex < _levelPrefabs.Count)
            {
                _currentLevelIndex = nextLevelIndex;
                StartLevel(_currentLevelIndex);
            }
            else
            {
                // If all levels are completed, choose a random level
                Debug.Log("All levels completed!");
                _currentLevelIndex = UnityEngine.Random.Range(0, _levelPrefabs.Count);
                StartLevel(_currentLevelIndex);
            }
        }

        /// <summary>
        /// Pauses the current level.
        /// </summary>
        public void PauseLevel()
        {
            if (!_isLevelInProgress) return;

            _isLevelInProgress = false;
            OnLevelPaused?.Invoke();
        }

        /// <summary>
        /// Fails the current level and reloads it.
        /// </summary>
        public void FailLevel()
        {
            if (!_isLevelInProgress) return;

            _isLevelInProgress = false;
            _currentLevel.FailLevel();
            OnLevelFailed?.Invoke();

            // Reload the current level
            StartLevel(_currentLevelIndex);
        }

        #endregion
    }
}
