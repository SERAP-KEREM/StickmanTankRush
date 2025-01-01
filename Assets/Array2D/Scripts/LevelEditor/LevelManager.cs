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
        [Tooltip("Current level index.")]
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

        private void Update()
        {
            // Test kazanma olay? (K tu?u)
            if (Input.GetKeyDown(KeyCode.K))
            {
                CompleteLevel();
            }

            // Test kaybetme olay? (F tu?u)
            if (Input.GetKeyDown(KeyCode.F))
            {
                FailLevel();
            }
        }
        #endregion

        #region Level Management

        /// <summary>
        /// Starts a level by index.
        /// </summary>
        /// <param name="levelIndex">Index of the level to start.</param>
        private void StartLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _levelPrefabs.Count)
            {
                Debug.LogWarning("Invalid level index provided.");
                return;
            }

            // E?er mevcut seviyemiz varsa, onu temizleyelim
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
            }

            _isLevelInProgress = true;

            // Yeni seviyeyi instantiate edelim
            Level newLevel = Instantiate(_levelPrefabs[levelIndex]);
            _currentLevel = newLevel;

            // Yeni seviyeyi LevelDataSO ile ba?lat
            LevelDataSO levelData = _levelDataList[levelIndex];
            newLevel.InitializeLevel(levelData);  // Bu fonksiyon Stickman'leri ve di?er nesneleri sahneye eklemeli

            // Seviye ba?lad??? eventini tetikle
            OnLevelStarted?.Invoke();

            // Seviye geçi? animasyonu
            newLevel.transform.localScale = Vector3.zero;
            newLevel.transform.DOScale(Vector3.one, _transitionDuration).SetEase(Ease.OutBounce);
        }

        /// <summary>
        /// Complete the current level and load the next level in sequence.
        /// </summary>
        public void CompleteLevel()
        {
            if (!_isLevelInProgress) return;

            _isLevelInProgress = false;
            _currentLevel.CompleteLevel();
            OnLevelCompleted?.Invoke();

            // Sonraki seviyeyi yükleyelim
            int nextLevelIndex = _currentLevelIndex + 1;
            if (nextLevelIndex < _levelPrefabs.Count)
            {
                _currentLevelIndex = nextLevelIndex;
                StartLevel(_currentLevelIndex);
            }
            else
            {
                Debug.Log("Tüm seviyeler tamamland?!");
                // ?sterseniz ilk seviyeye geri dönebilirsiniz
                _currentLevelIndex = 0;
                StartLevel(_currentLevelIndex);
            }
        }

        /// <summary>
        /// Fail the current level and reload the same level.
        /// </summary>
        public void FailLevel()
        {
            if (!_isLevelInProgress) return;

            _isLevelInProgress = false;
            _currentLevel.FailLevel();
            OnLevelFailed?.Invoke();

            // Ayn? seviyeyi tekrar yükleyelim
            StartLevel(_currentLevelIndex);
        }

        #endregion
    }
}
