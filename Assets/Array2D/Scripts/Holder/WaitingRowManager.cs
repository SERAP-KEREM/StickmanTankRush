using _Main;
using _Main._Enums;
using _Main._Stickman.StickmanGrid;
using DG.Tweening;
using LevelEditor;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRowManager : MonoBehaviour
{
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

    private Holder[] waitingTiles; // Array to hold tiles
    public List<Holder> availableHolders = new List<Holder>(); // List of available holders

    private void Start()
    {
        InitializeWaitingRow();  // Initialize the waiting row when the game starts
    }

    /// <summary>
    /// Initializes the waiting row using the tile prefab and configuration from LevelDataSO.
    /// </summary>
    public void InitializeWaitingRow()
    {
        waitingTiles = new Holder[rowWidth];
        availableHolders.Clear(); // Ensure that the list is cleared before adding new holders

        for (int i = 0; i < rowWidth; i++)
        {
            // Calculate tile position and instantiate the tile
            CreateTileAtPosition(i);
        }
    }

    /// <summary>
    /// Helper method to create a tile at a specific position.
    /// </summary>
    /// <param name="index">The index of the tile in the row.</param>
    private void CreateTileAtPosition(int index)
    {
        // Calculate the position for the tile
        Vector3 tilePosition = rowStartPosition + Vector3.right * index * tileSpacing;

        // Instantiate the tile prefab
        GameObject tileObject = Instantiate(_tilePrefab, tilePosition, Quaternion.identity, transform);

        // Attempt to get the Holder component and add to lists
        Holder tileComponent = tileObject.GetComponent<Holder>();
        if (tileComponent != null)
        {
            waitingTiles[index] = tileComponent;
            availableHolders.Add(tileComponent); // Add each tile to the available holders list
            tileObject.name = $"Holder [{index}]";
            Debug.Log($"Tile created at position {tilePosition}.");
        }
        else
        {
            Debug.LogWarning("Tile prefab does not have a Holder component.");
        }
    }

    /// <summary>
    /// Finds the first available (empty) holder and moves the Stickman there.
    /// </summary>
    /// <param name="stickman">The Stickman to move.</param>
    /// <returns>The first available Holder, or null if none are available.</returns>
    public Holder MoveToNearestAvailableHolder(Stickman stickman)
    {
        // Loop through available holders to find the first one that isn't occupied
        foreach (Holder holder in availableHolders)
        {
            if (!holder.IsOccupied)
            {
              
                holder.AssignStickman(stickman); // Assign the stickman to the holder
                availableHolders.Remove(holder); // Remove this holder from the available list
           
                Debug.Log($"Stickman {stickman.name} moved to Holder.");
                return holder; // Return the holder where the stickman was placed
            }
        }

        // Log warning if no available holder is found
        Debug.LogWarning("No available holder found for Stickman.");
        return null; // Return null if no available holder is found
    }
}
