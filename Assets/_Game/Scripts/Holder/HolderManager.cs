using UnityEngine;
using System.Collections.Generic;
using _Main._Stickman.StickmanGrid;
using TriInspector;
using _Main;

/// <summary>
/// Manages the creation and organization of holders for stickmen.
/// Handles holder availability and stickman placement logic.
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
    #endregion

    #region Events
    /// <summary>
    /// Triggered when all holders become occupied.
    /// </summary>
    public event System.Action OnAllHoldersFull;
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the holder manager and creates initial holders.
    /// </summary>
    public void Initialize()
    {
        InitializeWaitingRow();
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
    /// Checks if all holders are currently occupied.
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
            Debug.Log("[HolderManager] All holders are full!");
            OnAllHoldersFull?.Invoke();
        }

        return allFull;
    }
    #endregion

    #region Private Methods
    private void InitializeWaitingRow()
    {
        if (!ValidateSetup()) return;

        CleanupExistingHolders();
        CreateHolders();
    }

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

    private void CreateHolders()
    {
        for (int i = 0; i < _rowWidth; i++)
        {
            CreateHolderAtPosition(i);
        }
    }

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

    private void ConfigureHolder(Holder holder, int index)
    {
        holder.name = $"Holder [{index}]";
        _waitingHolders[index] = holder;
        _availableHolders.Add(holder);
    }

    private bool ValidateStickmanMovement(Stickman stickman)
    {
        if (stickman == null)
        {
            Debug.LogError("[HolderManager] Cannot move null stickman!");
            return false;
        }
        return true;
    }

    private bool ValidateGridPathFinder(GridPathFinder gridPathFinder)
    {
        if (gridPathFinder == null)
        {
            Debug.LogError("[HolderManager] GridPathFinder not found!");
            return false;
        }
        return true;
    }

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

    private Holder HandlePathfindingMove(Stickman stickman, GridPathFinder gridPathFinder)
    {
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
    #endregion

    #region Debug Methods
    [Button("Log Holder Status")]
    private void LogHolderStatus()
    {
        Debug.Log($"[HolderManager] Total Holders: {_waitingHolders?.Length ?? 0}");
        Debug.Log($"[HolderManager] Available Holders: {_availableHolders?.Count ?? 0}");

        if (_waitingHolders != null)
        {
            for (int i = 0; i < _waitingHolders.Length; i++)
            {
                var holder = _waitingHolders[i];
                string status = holder != null ?
                    (holder.IsOccupied ? "Occupied" : "Empty") :
                    "Null";
                Debug.Log($"[HolderManager] Holder [{i}]: {status}");
            }
        }
    }
    #endregion
}