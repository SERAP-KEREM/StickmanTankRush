using UnityEngine;
using System.Collections.Generic;
using TriInspector;
using System.Collections;

namespace _Main._Stickman.StickmanGrid
{
    /// <summary>
    /// Manages the creation and organization of holders for stickmen.
    /// Handles holder availability, stickman placement, and game over checks.
    /// </summary>
    [DeclareFoldoutGroup("Configuration", Title = "Holder Settings")]
    [DeclareFoldoutGroup("Runtime", Title = "Runtime References")]
    public class HolderManager : MonoBehaviour
    {
        #region Configuration

        [Group("Configuration")]
        [SerializeField, Required]
        [PropertyTooltip("Prefab used to create holder instances")]
        private Holder _holderPrefab;

        [Group("Configuration")]
        [SerializeField]
        [PropertyTooltip("Number of holders to create in a row")]
        [Range(1, 10)]
        private int _rowWidth = 5;

        [Group("Configuration")]
        [SerializeField]
        [PropertyTooltip("Space between each holder")]
        [Range(0.5f, 3f)]
        private float _holderSpacing = 1f;

        [Group("Configuration")]
        [SerializeField]
        [PropertyTooltip("Starting position for the first holder")]
        private Vector3 _rowStartPosition = Vector3.zero;

        #endregion

        #region Runtime References

        [Group("Runtime")]
        [SerializeField, ReadOnly]
        private Holder[] _waitingHolders;

        [Group("Runtime")]
        [SerializeField, ReadOnly]
        private List<Holder> _availableHolders = new List<Holder>();

        private Coroutine _failCheckCoroutine;

        #endregion

        #region Events

        /// <summary>
        /// Triggered when all holders become occupied.
        /// </summary>
        public event System.Action OnAllHoldersFull;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the holder manager and creates the initial set of holders.
        /// </summary>
        public void InitializeWaitingRow()
        {
            if (!ValidateSetup()) return;

            CleanupExistingHolders();
            CreateHolders();
        }

        /// <summary>
        /// Returns a list of all active holders.
        /// </summary>
        public List<Holder> GetAllHolders()
        {
            var allHolders = new List<Holder>();

            if (_waitingHolders != null)
            {
                foreach (var holder in _waitingHolders)
                {
                    if (holder != null)
                    {
                        allHolders.Add(holder);
                    }
                }
            }
            return allHolders;
        }

        /// <summary>
        /// Moves a stickman to the nearest available holder.
        /// </summary>
        public Holder MoveToNearestAvailableHolder(Stickman stickman)
        {
            if (!ValidateStickmanMovement(stickman)) return null;

            var gridPathFinder = FindObjectOfType<GridPathFinder>();
            if (!ValidateGridPathFinder(gridPathFinder)) return null;

            return stickman.GridY == 0
                ? HandleDirectMove(stickman)
                : HandlePathfindingMove(stickman, gridPathFinder);
        }

        /// <summary>
        /// Checks if all holders are currently occupied with a delay to avoid false fails.
        /// </summary>
        public bool AreAllHoldersFull()
        {
            if (_waitingHolders == null || _waitingHolders.Length == 0)
                return false;

            bool allFull = true;
            foreach (var holder in _waitingHolders)
            {
                if (holder != null && !holder.IsOccupied)
                {
                    allFull = false;
                    break;
                }
            }

            if (allFull)
            {
                Debug.Log("[HolderManager] All holders are full! Starting delayed check...");
                if (_failCheckCoroutine != null)
                {
                    StopCoroutine(_failCheckCoroutine); 
                }
                _failCheckCoroutine = StartCoroutine(DelayedFailCheck()); 
            }

            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the initial setup and required configurations.
        /// </summary>
        private bool ValidateSetup()
        {
            if (_holderPrefab == null)
            {
                Debug.LogError("[HolderManager] Holder prefab is not assigned!");
                return false;
            }

            if (_rowWidth <= 0)
            {
                Debug.LogError("[HolderManager] Row width must be greater than 0!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cleans up any existing holders before creating new ones.
        /// </summary>
        private void CleanupExistingHolders()
        {
            if (_waitingHolders != null)
            {
                foreach (var holder in _waitingHolders)
                {
                    if (holder != null)
                    {
                        Destroy(holder.gameObject);
                    }
                }
            }

            _waitingHolders = new Holder[_rowWidth];
            _availableHolders.Clear();
        }

        /// <summary>
        /// Creates the holders and initializes them.
        /// </summary>
        private void CreateHolders()
        {
            for (int i = 0; i < _rowWidth; i++)
            {
                CreateHolderAtPosition(i);
            }
        }

        /// <summary>
        /// Instantiates a holder at a specific position.
        /// </summary>
        private void CreateHolderAtPosition(int index)
        {
            Vector3 position = _rowStartPosition + Vector3.right * index * _holderSpacing;

            Holder holder = Instantiate(_holderPrefab, position, Quaternion.identity, transform);
            if (holder != null)
            {
                ConfigureHolder(holder, index);
            }
            else
            {
                Debug.LogError($"[HolderManager] Failed to create holder at index {index}");
            }
        }

        /// <summary>
        /// Configures a holder's properties after instantiation.
        /// </summary>
        private void ConfigureHolder(Holder holder, int index)
        {
            holder.name = $"Holder [{index}]";
            _waitingHolders[index] = holder;
            _availableHolders.Add(holder);
        }

        /// <summary>
        /// Validates the stickman movement logic.
        /// </summary>
        private bool ValidateStickmanMovement(Stickman stickman)
        {
            if (stickman == null)
            {
                Debug.LogError("[HolderManager] Cannot move null stickman!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates the GridPathFinder component.
        /// </summary>
        private bool ValidateGridPathFinder(GridPathFinder gridPathFinder)
        {
            if (gridPathFinder == null)
            {
                Debug.LogError("[HolderManager] GridPathFinder not found!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Moves a stickman directly to the nearest holder without pathfinding.
        /// </summary>
        private Holder HandleDirectMove(Stickman stickman)
        {
            foreach (var holder in _availableHolders)
            {
                if (holder != null && !holder.IsOccupied && holder.AssignStickman(stickman))
                {
                    Debug.Log($"[HolderManager] Direct move to {holder.name} successful");
                    AreAllHoldersFull();
                    return holder;
                }
            }
            return null;
        }

        /// <summary>
        /// Moves a stickman to a holder using pathfinding.
        /// </summary>
        private Holder HandlePathfindingMove(Stickman stickman, GridPathFinder gridPathFinder)
        {
            if (stickman.IsInHolder)
            {
                return HandleDirectMove(stickman);
            }

            foreach (var holder in _availableHolders)
            {
                if (holder != null && !holder.IsOccupied &&
                    gridPathFinder.HasValidPathToTarget(stickman) &&
                    holder.AssignStickman(stickman))
                {
                    Debug.Log($"[HolderManager] Pathfinding move to {holder.name} successful");
                    AreAllHoldersFull();
                    return holder;
                }
            }
            return null;
        }

        /// <summary>
        /// Delayed recheck to confirm if all holders are occupied after a short delay.
        /// </summary>
        private IEnumerator DelayedFailCheck()
        {
            yield return new WaitForSeconds(1.7f); 

            bool stillFull = true;
            foreach (var holder in _waitingHolders)
            {
                if (holder != null && !holder.IsOccupied)
                {
                    stillFull = false;
                    break;
                }
            }

            if (stillFull)
            {
                Debug.Log("[HolderManager] All holders are still full after delay - Game Over!");
                OnAllHoldersFull?.Invoke();
            }
            else
            {
                Debug.Log("[HolderManager] Delayed check passed, holders are no longer full.");
            }
        }

        #endregion
    }
}
