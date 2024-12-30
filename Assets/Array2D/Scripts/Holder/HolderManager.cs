using _Main;
using _Main._Enums;
using _Main._Stickman.StickmanGrid;
using DG.Tweening;
using LevelEditor;
using System.Collections.Generic;
using UnityEngine;

public class HolderManager : MonoBehaviour
{
    #region Fields

    [Header("Waiting Row Configuration")]
    [SerializeField, Tooltip("Holder prefab reference.")]
    private Holder _holderPrefab; // Holder prefab reference

    [SerializeField, Tooltip("Number of holders in the waiting row.")]
    private int _rowWidth; // Number of holders in the waiting row

    [SerializeField, Tooltip("Spacing between holders in the row.")]
    private float _holderSpacing = 1f; // Spacing between holders

    [SerializeField, Tooltip("Starting position for the waiting row.")]
    private Vector3 _rowStartPosition; // Starting position for the row

    [Header("Holder Data")]
    [Tooltip("Array to hold holders in the waiting row.")]
    private Holder[] _waitingHolders; // Array of waiting holders

    [SerializeField, Tooltip("List of available holders for stickmen.")]
    private List<Holder> _availableHolders = new List<Holder>(); // List of available holders for stickmen

    #endregion

    #region Unity Methods

    private void Start()
    {
        InitializeWaitingRow(); // Initialize the waiting row when the game starts
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the waiting row using the holder prefab and configuration from LevelDataSO.
    /// </summary>
    public void InitializeWaitingRow()
    {
        _waitingHolders = new Holder[_rowWidth];
        _availableHolders.Clear(); // Clear the list before adding new holders to avoid stale data

        // Create holders based on the row configuration
        for (int i = 0; i < _rowWidth; i++)
        {
            CreateHolderAtPosition(i);
        }
    }

    /// <summary>
    /// Retrieves all holders currently managed by this manager.
    /// </summary>
    public List<Holder> GetAllHolders()
    {
        List<Holder> allHolders = new List<Holder>();

        // Combine both waitingHolders and availableHolders lists
        allHolders.AddRange(_waitingHolders);
        allHolders.AddRange(_availableHolders);

        return allHolders;
    }

    /// <summary>
    /// Finds the first available (empty) holder and moves the Stickman there.
    /// </summary>
    /// <param name="stickman">The Stickman to move.</param>
    /// <returns>The first available Holder, or null if none are available.</returns>
    public Holder MoveToNearestAvailableHolder(Stickman stickman)
    {
        // Iterate through available holders and find the first one that's empty
        foreach (Holder holder in _availableHolders)
        {
            if (!holder.IsOccupied)
            {
                holder.AssignStickman(stickman); // Assign Stickman to the holder
                Debug.Log($"Stickman {stickman.name} moved to Holder.");
                return holder; // Return the holder where Stickman was placed
            }
        }

        // Log a warning if no available holder is found
        Debug.LogWarning("No available holder found for Stickman.");
        return null; // Return null if no available holder is found
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Creates a holder at a specific position in the row.
    /// </summary>
    /// <param name="index">The index of the holder in the row.</param>
    private void CreateHolderAtPosition(int index)
    {
        // Calculate the position for the holder using the starting position and spacing
        Vector3 holderPosition = _rowStartPosition + Vector3.right * index * _holderSpacing;

        // Instantiate the holder prefab at the calculated position
        Holder holderComponent = Instantiate(_holderPrefab, holderPosition, Quaternion.identity, transform);

        // Configure the Holder component
        if (holderComponent != null)
        {
            _waitingHolders[index] = holderComponent;
            _availableHolders.Add(holderComponent); // Add the holder to the list of available holders
            holderComponent.name = $"Holder [{index}]"; // Set a descriptive name for the holder object

            Debug.Log($"Holder created at position {holderPosition}.");
        }
        else
        {
            Debug.LogWarning("Holder prefab instantiation failed.");
        }
    }

    #endregion
}
