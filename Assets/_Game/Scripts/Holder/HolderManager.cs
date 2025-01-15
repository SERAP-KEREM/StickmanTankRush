using UnityEngine;
using System.Collections.Generic;
using _Main._Stickman.StickmanGrid;
using _Main._Enums;
using _Main;
using DG.Tweening;
using LevelEditor;

public class HolderManager : MonoBehaviour
{
    #region Fields
    [Header("Holder Configuration")]
    [SerializeField, Tooltip("Holder prefab reference")]
    private Holder _holderPrefab;

    [SerializeField, Tooltip("Number of holders in the row")]
    private int _rowWidth = 5;

    [SerializeField, Tooltip("Spacing between holders")]
    private float _holderSpacing = 1f;

    [SerializeField, Tooltip("Starting position for holders")]
    private Vector3 _rowStartPosition = Vector3.zero;

    private Holder[] _waitingHolders;
    private List<Holder> _availableHolders = new List<Holder>();
    #endregion
    public event System.Action OnAllHoldersFull;


    #region Initialization
    public void Initialize()
    {

        InitializeWaitingRow();
    }
    public void InitializeWaitingRow()
    {
        if (!ValidateSetup()) return;

        CleanupExistingHolders();
        CreateHolders();
    }

    private bool ValidateSetup()
    {
        if (_holderPrefab == null)
        {
            Debug.LogError("Holder prefab is not assigned!", this);
            return false;
        }

        if (_rowWidth <= 0)
        {
            Debug.LogError("Row width must be greater than 0!", this);
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
    /// <summary>
    /// Checks if all holders are currently occupied
    /// </summary>
    public bool AreAllHoldersFull()
    {
        if (_waitingHolders == null || _waitingHolders.Length == 0)
            return false;

        // Tüm holder'lar? kontrol et
        foreach (var holder in _waitingHolders)
        {
            // E?er bir holder bo?sa, hepsi dolu de?il demektir
            if (holder != null && !holder.IsOccupied)
            {
                return false;
            }
        }

        // Tüm holder'lar doluysa true döndür
        Debug.Log("All holders are full! Game Over condition met.");
        OnAllHoldersFull?.Invoke();
        return true;
    }

    private void CreateHolders()
    {
        for (int i = 0; i < _rowWidth; i++)
        {
            CreateHolderAtPosition(i);
        }
        //Debug.Log($"Created {_rowWidth} holders in waiting row");
    }
    #endregion

    #region Holder Management
    public List<Holder> GetAllHolders()
    {
        List<Holder> allHolders = new List<Holder>();

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

        if (_availableHolders != null)
        {
            foreach (var holder in _availableHolders)
            {
                if (holder != null && !allHolders.Contains(holder))
                {
                    allHolders.Add(holder);
                }
            }
        }

        return allHolders;
    }

    public Holder MoveToNearestAvailableHolder(Stickman stickman)
    {
        if (stickman == null)
        {
            Debug.LogError("[HolderManager] Cannot move null stickman!");
            return null;
        }

        // GridPathFinder kontrolü
        var gridPathFinder = FindObjectOfType<GridPathFinder>();
        if (gridPathFinder == null)
        {
            Debug.LogError("[HolderManager] GridPathFinder not found!");
            return null;
        }

        // z=0 kontrolü
        if (stickman.GridY == 0)
        {
            foreach (Holder holder in _availableHolders)
            {
                if (holder != null && !holder.IsOccupied)
                {
                    bool success = holder.AssignStickman(stickman);
                    if (success)
                    {
                        Debug.Log($"[HolderManager] Direct move to holder {holder.name} (z=0)");
                        AreAllHoldersFull();
                        return holder;
                    }
                }
            }
            return null;
        }

        // Yol kontrolü ve hareket
        foreach (Holder holder in _availableHolders)
        {
            if (holder != null && !holder.IsOccupied)
            {
                // Önce yol kontrolü yap
                if (gridPathFinder.HasValidPathToTarget(stickman))
                {
                    bool success = holder.AssignStickman(stickman);
                    if (success)
                    {
                        Debug.Log($"[HolderManager] Moved stickman to holder {holder.name}");
                        AreAllHoldersFull();
                        return holder;
                    }
                }
                else
                {
                    Debug.Log($"[HolderManager] No valid path to holder {holder.name}");
                }
            }
        }

        Debug.LogWarning("[HolderManager] No available holder or valid path found");
        return null;
    }


    #endregion

    #region Helper Methods
    private void CreateHolderAtPosition(int index)
    {
        Vector3 position = _rowStartPosition + Vector3.right * index * _holderSpacing;

        Holder holder = Instantiate(_holderPrefab, position, Quaternion.identity, transform);

        if (holder != null)
        {
            holder.name = $"Holder [{index}]";
            _waitingHolders[index] = holder;
            _availableHolders.Add(holder);

          //  Debug.Log($"Created {holder.name} at position {position}");
        }
        else
        {
            Debug.LogError($"Failed to create holder at index {index}");
        }
    }
    #endregion

    #region Debug
    public void LogHolderStatus()
    {
        Debug.Log($"Total Waiting Holders: {_waitingHolders?.Length ?? 0}");
        Debug.Log($"Available Holders: {_availableHolders?.Count ?? 0}");

        if (_waitingHolders != null)
        {
            for (int i = 0; i < _waitingHolders.Length; i++)
            {
                var holder = _waitingHolders[i];
                string status = holder != null ?
                    (holder.IsOccupied ? "Occupied" : "Empty") :
                    "Null";
                Debug.Log($"Holder [{i}]: {status}");
            }
        }
    }
    #endregion
}