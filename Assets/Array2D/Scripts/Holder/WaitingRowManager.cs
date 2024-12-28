using _Main;
using _Main._Enums;
using _Main._Stickman.StickmanGrid;
using DG.Tweening;
using LevelEditor;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRowManager : MonoBehaviour
{
    #region Fields

    [Header("Waiting Row Configuration")]
    [SerializeField, Tooltip("Tile prefab reference.")]
    private GameObject _tilePrefab;

    [SerializeField, Tooltip("Level data for grid and row configuration.")]
    private LevelDataSO _levelDataSO;

    [SerializeField, Tooltip("Number of tiles in the waiting row.")]
    private int rowWidth;

    [SerializeField, Tooltip("Spacing between tiles in the row.")]
    private float tileSpacing = 1f;

    [SerializeField, Tooltip("Starting position for the waiting row.")]
    private Vector3 rowStartPosition;

    [Header("Tile Data")]
    [Tooltip("Array to hold tiles in the waiting row.")]
    private Holder[] waitingTiles;

    [SerializeField, Tooltip("List of available holders for stickmen.")]
    private List<Holder> availableHolders = new List<Holder>();

    #endregion

    #region Unity Methods

    private void Start()
    {
        InitializeWaitingRow();  // Initialize the waiting row when the game starts
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the waiting row using the tile prefab and configuration from LevelDataSO.
    /// </summary>
    public void InitializeWaitingRow()
    {
        waitingTiles = new Holder[rowWidth];
        availableHolders.Clear(); // Clear the list before adding new holders to avoid stale data

        // Create tiles based on the row configuration
        for (int i = 0; i < rowWidth; i++)
        {
            CreateTileAtPosition(i);
        }
    }

    /// <summary>
    /// Retrieves all stickmen currently in available holders.
    /// </summary>
    public List<Stickman> GetAllStickmenInHolders()
    {
        List<Stickman> stickmenInHolders = new List<Stickman>();

        // Iterate through each holder to check if it contains a Stickman
        foreach (var holder in availableHolders)
        {
            Stickman stickman = holder.GetStickman();
            if (stickman != null)
            {
                stickmenInHolders.Add(stickman); // Add Stickman to the list if found
            }
        }

        return stickmenInHolders;
    }

    /// <summary>
    /// Finds the first available (empty) holder and moves the Stickman there.
    /// </summary>
    /// <param name="stickman">The Stickman to move.</param>
    /// <returns>The first available Holder, or null if none are available.</returns>
    public Holder MoveToNearestAvailableHolder(Stickman stickman)
    {
        // Iterate through available holders and find the first one that's empty
        foreach (Holder holder in availableHolders)
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
    /// Creates a tile at a specific position in the row.
    /// </summary>
    /// <param name="index">The index of the tile in the row.</param>
    private void CreateTileAtPosition(int index)
    {
        // Calculate the position for the tile using the starting position and spacing
        Vector3 tilePosition = rowStartPosition + Vector3.right * index * tileSpacing;

        // Instantiate the tile prefab at the calculated position
        GameObject tileObject = Instantiate(_tilePrefab, tilePosition, Quaternion.identity, transform);

        // Try to get the Holder component from the instantiated tile
        Holder tileComponent = tileObject.GetComponent<Holder>();
        if (tileComponent != null)
        {
            waitingTiles[index] = tileComponent;
            availableHolders.Add(tileComponent); // Add the tile to the list of available holders
            tileObject.name = $"Holder [{index}]"; // Set a descriptive name for the tile object

            Debug.Log($"Tile created at position {tilePosition}.");
        }
        else
        {
            Debug.LogWarning("Tile prefab does not have a Holder component.");
        }
    }

    #endregion
}
