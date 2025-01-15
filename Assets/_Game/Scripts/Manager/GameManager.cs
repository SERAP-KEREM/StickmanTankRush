using _Main._Stickman.StickmanGrid;
using _Main._Tank;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SerapKeremGameTools._Game._Singleton;
using SerapKeremGameTools._Game._AudioSystem;
using System.Collections;
using UnityEngine.AI;

namespace _Main
{
    /// <summary>
    /// Main controller for managing the game logic, Stickman interactions, tanks, and movement.
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        #region Field References

        [Header("Game References")]
        [Tooltip("Manages all tank operations in the game.")]
        private TankManager _tankManager;

        [Tooltip("Handles the Stickman grid structure.")]
        private StickmanGrid _stickmanGrid;

        [Tooltip("Handles the Tile grid structure.")]
        private TileGrid _tileGrid;

        [Tooltip("Manages the holders for waiting Stickmen.")]
        private HolderManager _holderManager;

        #endregion

        #region Private Fields

        [Header("Private State Variables")]
        [SerializeField, Tooltip("Currently selected Stickman.")]
        private Stickman _selectedStickman;

        #endregion
        [Header("UI References")]
        [SerializeField] private GameplayUI _gameplayUI;
        [Header("Score System")]
        private float _lastMatchTime;
        #region Unity Lifecycle Methods

        #region Game State
        private bool _isPaused;
        #endregion

        GridPathFinder _gridPathFinder;
        protected override void Awake()
        {
            base.Awake();
          
            InitializeAudioManager();
        }
        private void InitializeAudioManager()
        {
            if (AudioManager.Instance == null)
            {
                // Resources'dan AudioManager prefabını yükle
                var audioManagerPrefab = Resources.Load<AudioManager>("Prefabs/AudioManager");

                if (audioManagerPrefab != null)
                {
                    var audioManager = Instantiate(audioManagerPrefab);
                    audioManager.name = "AudioManager";
                    Debug.Log("[GameManager] AudioManager created from Resources");
                }
                else
                {
                    Debug.LogError("[GameManager] AudioManager prefab not found in Resources/Prefabs!");
                }
            }

            StartCoroutine(InitializeAudioDelayed());
        }

        private IEnumerator InitializeAudioDelayed()
        {
            yield return new WaitForSeconds(0.1f);

            if (AudioManager.Instance != null)
            {
                InitializeAudio();
            }
        }
        private void InitializeAudio()
        {
            AudioManager.Instance.PlayAudio(AudioKeys.GAME_MUSIC);
            AudioManager.Instance.PlayAudio(AudioKeys.GAME_START);
        }
        #endregion

        #region Tank Progress Updates
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

        #region Reference Validation

        /// <summary>
        /// Validates the assigned references for required components.
        /// </summary>
        private void ValidateReferences()
        {
            if (_tankManager == null) { Debug.LogError("TankManager is not assigned!"); return; }
            if (_stickmanGrid == null) { Debug.LogError("StickmanGrid is not assigned!"); return; }
            if (_tileGrid == null) { Debug.LogError("TileGrid is not assigned!"); return; }
            if (_holderManager == null) { Debug.LogError("HolderManager is not assigned!"); return; }
            if (_gridPathFinder == null) { Debug.LogError("GridPathFinder is not assigned!"); return; }
            //  Debug.Log("References validated.");
        }

        #endregion

        #region Stickman Handling

        /// <summary>
        /// Initializes the game with the provided level, updating necessary references.
        /// </summary>
        /// <param name="level">The level data containing all relevant references.</param>
        public void OnLevelCreated(Level level)
        {
            if (level == null)
            {
                Debug.LogError("Level reference is null!");
                return;
            }

            // Update references
            _tankManager = level.TankManager;
            _stickmanGrid = level.StickmanGrid;
            _tileGrid = level.TileGrid;
            _holderManager = level.HolderManager;
            _gridPathFinder = level.GridPathFinder;
            if (_gameplayUI != null)
            {
                _gameplayUI.Show();
                _gameplayUI.ResetProgress();
            }
            if (_gridPathFinder == null)
            {
                Debug.LogError("[GameManager] GridPathFinder is missing!");
                return;
            }
            _gridPathFinder.Initialize(_tileGrid);
            // Validate references
            ValidateReferences();

            // Initialize grids if references are valid
            _stickmanGrid?.Initialize();
            _tileGrid?.Initialize();
        }
        private bool CanMoveToTarget(Stickman stickman, Tank tank)
        {
            if (stickman == null || tank == null) return false;
            if (tank.IsFull || tank.UnitColorType != stickman.UnitColorType) return false;

            NavMeshPath path = new NavMeshPath();
            Vector3 targetPos = tank.GetStickmanTargetPosition();

            return NavMesh.CalculatePath(stickman.transform.position, targetPos, NavMesh.AllAreas, path);
        }

        public NavMeshPath GetPath(Vector3 start, Vector3 end)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
            {
                return path;
            }
            return null;
        }

        /// <summary>
        /// Handles the selection of a Stickman by the player.
        /// </summary>
        /// <param name="stickman">The selected Stickman instance.</param>
        public void HandleStickmanSelection(Stickman stickman)
        {
            if (!ValidateStickmanSelection(stickman)) return;

            AudioManager.Instance.PlayAudio(AudioKeys.STICKMAN_CLICK);
            _selectedStickman = stickman;

            Tank currentTank = _tankManager.CurrentTank;
            if (currentTank == null) return;

            // Renk kontrolü
            if (currentTank.UnitColorType != stickman.UnitColorType)
            {
                HandleColorMismatch(stickman);
                return;
            }

            // Tank dolu mu?
            if (currentTank.IsFull)
            {
                Debug.Log("Tank is full!");
                return;
            }

            // z=0 kontrolü (en öndeki stickman)
            if (stickman.GridY == 0)
            {
                MoveStickmanToTank(stickman, currentTank);
                return;
            }

            // Yol kontrolü
            if (_gridPathFinder.HasValidPathToTarget(stickman))
            {
                MoveStickmanToTank(stickman, currentTank);
            }
            else
            {
                Debug.Log("No valid path to tank!");
            }
        }

        private bool ValidateStickmanSelection(Stickman stickman)
        {
            // Check if components are valid
            if (_holderManager == null)
            {
                Debug.LogWarning("[GameManager] HolderManager is null!");
                return false;
            }

            if (stickman == null)
            {
                Debug.LogWarning("[GameManager] Selected stickman is null!");
                return false;
            }

            if (!stickman.IsSelectable)
            {
                Debug.LogWarning("[GameManager] Stickman is not selectable!");
                return false;
            }

            // Check if tile grid is valid
            if (_tileGrid == null)
            {
                Debug.LogWarning("[GameManager] TileGrid is null!");
                return false;
            }

            // Check if neighbors are empty (optional, based on your game rules)
            if (!_tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                Debug.LogWarning("[GameManager] No empty neighbors for stickman!");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Handles the color mismatch situation and moves the Stickman to the nearest available holder.
        /// </summary>
        /// <param name="stickman">The Stickman to move.</param>
        private void HandleColorMismatch(Stickman stickman)
        {
            if (stickman == null) return;

            // GridPathFinder kontrolü
            if (_gridPathFinder == null)
            {
                Debug.LogError("[GameManager] GridPathFinder is null!");
                return;
            }

            // Yol kontrolü
            if (!_gridPathFinder.HasValidPathToTarget(stickman))
            {
                Debug.Log("[GameManager] No valid path for stickman to holder");
                return;
            }

            // Move Stickman to the nearest available holder
            Holder nearestHolder = _holderManager.MoveToNearestAvailableHolder(stickman);

            if (nearestHolder != null)
            {
                ScoreManager.Instance.OnHolderUsed();
                ProcessStickmanMovement(stickman, null, nearestHolder);
                Debug.Log($"[GameManager] Successfully moved stickman to holder {nearestHolder.name}");
            }
            else
            {
                Debug.LogWarning("[GameManager] Failed to move stickman to holder");
            }
        }

        /// <summary>
        /// Moves the Stickman to the appropriate Tank.
        /// </summary>
        /// <param name="stickman">The Stickman to move.</param>
        /// <param name="currentTank">The tank to move the Stickman to.</param>
        private void MoveStickmanToTank(Stickman stickman, Tank currentTank)
        {
            if (stickman == null || currentTank == null) return;

            ProcessStickmanMovement(stickman, currentTank);
            //Debug.Log($"Stickman moved to tank. Current count: {currentTank.StickmanCount}");
            UpdateTankProgress(currentTank);

            float matchTime = Time.time - _lastMatchTime;
            if (matchTime < 1.5f)
            {
                ScoreManager.Instance.OnQuickMatch();
            }
            _lastMatchTime = Time.time;

            // Check if the tank is full
            if (currentTank.IsFull)
            {
                ScoreManager.Instance.OnTankCompleted();
                // Debug.Log("Tank is full, moving to next tank...");
                MoveNextTankToStopPoint();
            }
        }

        /// <summary>
        /// Processes the movement of a Stickman (either to the Tank or Holder).
        /// </summary>
        /// <param name="stickman">The Stickman to move.</param>
        /// <param name="currentTank">The current Tank (optional, can be null for Holder).</param>
        /// <param name="nearestHolder">The nearest available holder (optional, can be null).</param>
        private void ProcessStickmanMovement(Stickman stickman, Tank currentTank = null, Holder nearestHolder = null)
        {
            if (stickman.GridY == 0)
            {
                MoveStickmanDirectly(stickman, currentTank, nearestHolder);
                return;
            }
            if (!_gridPathFinder.HasValidPathToTarget(stickman))
            {
                Debug.Log("[GameManager] No valid path found, movement cancelled");
                return;
            }
            // Remove Stickman from the current tile
            Tile currentTile = _tileGrid.GetTileAt(stickman.GridX, stickman.GridY);
            if (currentTile != null)
            {
                currentTile.RemoveStickman();
                //Debug.Log($"Removed stickman from tile ({stickman.GridX}, {stickman.GridY})");
            }
          
            if (nearestHolder != null)
            {
                // Assign Stickman to the nearest holder
                nearestHolder.AssignStickman(stickman);
                stickman.IsSelectable = false;
               // Debug.Log($"Assigned stickman to holder {nearestHolder.name}");
            }
            else if (currentTank != null)
            {
                stickman.MoveToTank(currentTank.GetStickmanTargetPosition(), currentTank.transform);
                currentTank.AddStickman(stickman.UnitColorType);
           
                // Debug.Log($"Added stickman to tank. Tank color: {currentTank.UnitColorType}");
            }
        }
        private void MoveStickmanDirectly(Stickman stickman, Tank currentTank, Holder nearestHolder)
        {
            if (nearestHolder != null)
            {
                stickman.MoveToHolder(nearestHolder.transform.position);
                nearestHolder.AssignStickman(stickman);
                stickman.IsSelectable = false;
            }
            else if (currentTank != null)
            {
                stickman.MoveToTank(currentTank.GetStickmanTargetPosition(), currentTank.transform);
                currentTank.AddStickman(stickman.UnitColorType);
            }
        }
        private void MoveNextTankToStopPoint()
        {
            Tank currentTank = _tankManager.CurrentTank;

            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

           // Debug.Log("Moving to the next tank.");
            _tankManager.MoveNextTankToStopPoint(); // Move the next tank to its stop point
            _tankManager.MoveOtherTanks(); // Reorganize other tanks

            UpdateTankProgress(_tankManager.CurrentTank);

            MoveAllHolderStickmenToCurrentTank();
        }

        private void MoveAllHolderStickmenToCurrentTank()
        {
            Tank currentTank = _tankManager.CurrentTank;

            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            List<Holder> allHolders = _holderManager.GetAllHolders();
            foreach (var holder in allHolders)
            {
                Stickman stickmanInHolder = holder.CurrentStickman;

                if (stickmanInHolder != null &&
                    !currentTank.IsFull &&
                    stickmanInHolder.UnitColorType == currentTank.UnitColorType) // Color match check
                {
                    holder.RemoveStickman();
                    MoveStickmanToTank(stickmanInHolder, currentTank);
                }
            }
        }

        #endregion
    }
}
