using UnityEngine;
using _Main._Tank;
using _Main._Stickman.StickmanGrid;
using SerapKeremGameTools._Game._Singleton;
using SerapKeremGameTools._Game._AudioSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using TriInspector;
using DG.Tweening;

namespace _Main
{
    /// <summary>
    /// Main controller for managing game logic, stickman interactions, and tank operations.
    /// Implements the Singleton pattern for global access.
    /// </summary>
    [DeclareFoldoutGroup("References", Title = "Game References")]
    [DeclareFoldoutGroup("State", Title = "Game State")]
    [DeclareFoldoutGroup("UI", Title = "UI Elements")]
    public class GameManager : MonoSingleton<GameManager>
    {
        #region Dependencies
        [Group("References")]
        [SerializeField, Required]
        [PropertyTooltip("Manages all tank operations")]
        private TankManager _tankManager;

        [Group("References")]
        [SerializeField, Required]
        [PropertyTooltip("Handles the stickman grid structure")]
        private StickmanGrid _stickmanGrid;

        [Group("References")]
        [SerializeField, Required]
        [PropertyTooltip("Handles the tile grid structure")]
        private TileGrid _tileGrid;

        [Group("References")]
        [SerializeField, Required]
        [PropertyTooltip("Manages holders for waiting stickmen")]
        private HolderManager _holderManager;

        [Group("References")]
        [SerializeField, Required]
        private GridPathFinder _gridPathFinder;
        #endregion

        #region State Variables
        [Group("State")]
        [SerializeField, ReadOnly]
        private Stickman _selectedStickman;

        [Group("State")]
        [SerializeField]
        private bool _isPaused;

        [Group("State")]
        [SerializeField, ReadOnly]
        private float _lastMatchTime;
        #endregion

        #region UI References
        [Group("UI")]
        [SerializeField, Required]
        private GameplayUI _gameplayUI;
        #endregion

        #region Unity Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();
            InitializeAudioManager();
        }
        #endregion

        #region Initialization Methods
        private void InitializeAudioManager()
        {
            if (AudioManager.Instance != null) return;

            var audioManagerPrefab = Resources.Load<AudioManager>("Prefabs/AudioManager");
            if (audioManagerPrefab == null)
            {
                Debug.LogError("[GameManager] AudioManager prefab not found!");
                return;
            }

            var audioManager = Instantiate(audioManagerPrefab);
            audioManager.name = "AudioManager";
            StartCoroutine(InitializeAudioDelayed());
        }

        private IEnumerator InitializeAudioDelayed()
        {
            yield return new WaitForSeconds(0.1f);
            if (AudioManager.Instance != null)
            {
                PlayInitialAudio();
            }
        }

        private void PlayInitialAudio()
        {
            AudioManager.Instance.PlayAudio(AudioKeys.GAME_MUSIC);
            AudioManager.Instance.PlayAudio(AudioKeys.GAME_START);
        }

        /// <summary>
        /// Initializes the game with the provided level data.
        /// </summary>
        public void OnLevelCreated(Level level)
        {
            if (!ValidateLevel(level)) return;

            InitializeComponents(level);
            InitializeUI();
            InitializeGrids();
        }

        private bool ValidateLevel(Level level)
        {
            if (level == null)
            {
                Debug.LogError("[GameManager] Level reference is null!");
                return false;
            }
            return true;
        }
        private void InitializeComponents(Level level)
        {
            _tankManager = level.TankManager;
            _stickmanGrid = level.StickmanGrid;
            _tileGrid = level.TileGrid;
            _holderManager = level.HolderManager;
            _gridPathFinder = level.GridPathFinder;

            //if (_gridPathFinder != null)
            //{
            //    _gridPathFinder.Initialize(_tileGrid);
            //}

            ValidateReferences();
        }

        private void InitializeUI()
        {
            if (_gameplayUI != null)
            {
                _gameplayUI.Show();
                _gameplayUI.ResetProgress();
            }
        }

        private void InitializeGrids()
        {
            _stickmanGrid?.Initialize();
            _tileGrid?.Initialize();
        }
        #endregion

        #region Reference Validation
        private void ValidateReferences()
        {
            if (_tankManager == null) Debug.LogError("[GameManager] TankManager is not assigned!");
            if (_stickmanGrid == null) Debug.LogError("[GameManager] StickmanGrid is not assigned!");
            if (_tileGrid == null) Debug.LogError("[GameManager] TileGrid is not assigned!");
            if (_holderManager == null) Debug.LogError("[GameManager] HolderManager is not assigned!");
            if (_gridPathFinder == null) Debug.LogError("[GameManager] GridPathFinder is not assigned!");
        }
        #endregion

        #region Tank Progress Management
        private void UpdateTankProgress(Tank tank)
        {
            if (_gameplayUI != null && tank != null)
            {
                _gameplayUI.UpdateTankProgress(
                    tank.StickmanCount,
                    tank.MaxStickmanCount,
                    tank.UnitColorType
                );
            }
        }
        #endregion

        #region Stickman Management
        public void HandleStickmanSelection(Stickman stickman)
        {
            if (!ValidateStickmanSelection(stickman)) return;

            AudioManager.Instance.PlayAudio(AudioKeys.STICKMAN_CLICK);
            _selectedStickman = stickman;

            Tank currentTank = _tankManager.CurrentTank;
            if (currentTank == null) return;

            ProcessStickmanSelection(stickman, currentTank);
        }

        private void ProcessStickmanSelection(Stickman stickman, Tank currentTank)
        {
            if (currentTank.UnitColorType != stickman.UnitColorType)
            {
                HandleColorMismatch(stickman);
                return;
            }

            if (currentTank.IsFull)
            {
                Debug.Log("[GameManager] Tank is full!");
                return;
            }

            if (stickman.GridY == 0 || _gridPathFinder.HasValidPathToTarget(stickman))
            {
                MoveStickmanToTank(stickman, currentTank);
            }
            else
            {
                Debug.Log("[GameManager] No valid path to tank!");
            }
        }

        private bool ValidateStickmanSelection(Stickman stickman)
        {
            if (_holderManager == null || stickman == null || !stickman.IsSelectable || _tileGrid == null)
            {
                return false;
            }

            return _tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY);
        }

        private void HandleColorMismatch(Stickman stickman)
        {
            if (stickman == null || _gridPathFinder == null) return;

            if (!_gridPathFinder.HasValidPathToTarget(stickman))
            {
                Debug.Log("[GameManager] No valid path for stickman to holder");
                return;
            }

            Holder nearestHolder = _holderManager.MoveToNearestAvailableHolder(stickman);
            if (nearestHolder != null)
            {
                ScoreManager.Instance.OnHolderUsed();
                ProcessStickmanMovement(stickman, null, nearestHolder);
            }
        }

        private void MoveStickmanToTank(Stickman stickman, Tank currentTank)
        {
            if (stickman == null || currentTank == null) return;

            ProcessStickmanMovement(stickman, currentTank);
            UpdateTankProgress(currentTank);
            CheckQuickMatch();

            if (currentTank.IsFull)
            {
                HandleFullTank(currentTank);
            }
        }

        private void CheckQuickMatch()
        {
            float matchTime = Time.time - _lastMatchTime;
            if (matchTime < 1.5f)
            {
                ScoreManager.Instance.OnQuickMatch();
            }
            _lastMatchTime = Time.time;
        }

        private void HandleFullTank(Tank currentTank)
        {
            ScoreManager.Instance.OnTankCompleted();
            StartCoroutine(WaitAndMoveTank());
        }
        private IEnumerator WaitAndMoveTank()
        {
            // Tüm stickmanların tanka binmesini bekle
            yield return new WaitForSeconds(0.3f);

            MoveNextTankToStopPoint();
        }
        private void ProcessStickmanMovement(Stickman stickman, Tank currentTank = null, Holder nearestHolder = null)
        {
            var currentTile = _tileGrid.GetTileAt(stickman.GridX, stickman.GridY);
            currentTile?.RemoveStickman();

            if (nearestHolder != null)
            {
                MoveStickmanToHolder(stickman, nearestHolder);
            }
            else if (currentTank != null)
            {
                MoveStickmanToCurrentTank(stickman, currentTank);
            }
        }
        private void MoveStickmanToHolder(Stickman stickman, Holder holder)
        {
            Vector3 targetPos = holder.GetStickmanTargetPosition();
            stickman.MoveToHolder(targetPos);
            holder.AssignStickman(stickman);
        }

        private void MoveStickmanToCurrentTank(Stickman stickman, Tank tank)
        {
            if (tank.IsFull) return;

            Vector3 targetPos = tank.GetStickmanTargetPosition();
            stickman.MoveToTank(targetPos, tank.transform);

            // Önce stickman'ın tanka varmasını bekle, sonra say
            StartCoroutine(WaitForStickmanArrival(stickman, tank));
        }

        private IEnumerator WaitForStickmanArrival(Stickman stickman, Tank tank)
        {
            // Stickman'ın hareketi tamamlanana kadar bekle
            while (stickman.IsMoving)
            {
                yield return null;
            }

            // Tank'a ekle ve UI'ı güncelle
            tank.AddStickman(stickman.UnitColorType);
            UpdateTankProgress(tank);

            // Tank doldu mu kontrol et
            if (tank.IsFull)
            {
                HandleFullTank(tank);
            }
        }

        #endregion

        #region Tank Movement Management
        private void MoveNextTankToStopPoint()
        {
            Tank currentTank = _tankManager.CurrentTank;
            if (currentTank == null)
            {
                Debug.LogWarning("[GameManager] No active tank available.");
                return;
            }

            _tankManager.MoveNextTankToStopPoint();
            _tankManager.MoveOtherTanks();
            UpdateTankProgress(_tankManager.CurrentTank);
           
            StartCoroutine(CheckHoldersAfterTankChange());
        }
        private IEnumerator CheckHoldersAfterTankChange()
        {
            yield return new WaitForSeconds(0.2f);

            Tank currentTank = _tankManager.CurrentTank;
            if (currentTank == null || currentTank.IsFull) yield break;

            yield return new WaitForSeconds(0.1f);

            if (currentTank != null && !currentTank.IsFull && currentTank.CurrentState == TankState.Filling)
            {
                Debug.Log("[GameManager] Tank ready, processing holder stickmen");
                yield return StartCoroutine(ProcessAllHolderStickmen(currentTank));
            }
        }

        private IEnumerator ProcessAllHolderStickmen(Tank currentTank)
        {
            var holders = _holderManager.GetAllHolders();
            foreach (var holder in holders)
            {
                if (currentTank.IsFull) break;

                Stickman stickmanInHolder = holder.CurrentStickman;
                if (stickmanInHolder != null && stickmanInHolder.UnitColorType == currentTank.UnitColorType)
                {
                    yield return StartCoroutine(ProcessSingleHolderStickman(holder, currentTank));
                }
            }
        }

        private IEnumerator ProcessSingleHolderStickman(Holder holder, Tank currentTank)
        {
            Stickman stickmanInHolder = holder.CurrentStickman;
            if (stickmanInHolder == null || currentTank.IsFull ||
                stickmanInHolder.UnitColorType != currentTank.UnitColorType)
                yield break;

            Debug.Log($"[GameManager] Moving stickman directly from holder to tank");

            // Holder'dan çıkar
            holder.RemoveStickman();

            // Tank'ın güncel pozisyonunu al
            Vector3 tankPos = currentTank.GetStickmanTargetPosition();

            // Direkt tank'a parent'la ve hareket ettir
            stickmanInHolder.transform.SetParent(currentTank.transform);
            stickmanInHolder.transform.DOMove(tankPos, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    stickmanInHolder.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    currentTank.OnStickmanArrived(stickmanInHolder);
                    currentTank.AddStickman(stickmanInHolder.UnitColorType);
                    UpdateTankProgress(currentTank);
                });

            yield return new WaitForSeconds(0.1f);
        }
        public void MoveAllHolderStickmenToCurrentTank()
        {
            Tank currentTank = _tankManager.CurrentTank;
            if (currentTank == null || currentTank.IsFull || currentTank.CurrentState != TankState.Filling)
            {
                Debug.Log("[GameManager] Cannot move holder stickmen: Tank not ready");
                return;
            }

            StartCoroutine(ProcessAllHolderStickmen(currentTank));
        }

      
        #endregion
    }
}
