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

    private Holder[] waitingTiles;
    public List<Holder> availableHolders = new List<Holder>();

    private void Start()
    {
        InitializeWaitingRow();
    }

    /// <summary>
    /// Initializes the waiting row using the tile prefab and configuration from LevelDataSO.
    /// </summary>
    public void InitializeWaitingRow()
    {
        waitingTiles = new Holder[rowWidth];

        for (int i = 0; i < rowWidth; i++)
        {
            // Calculate tile position
            Vector3 tilePosition = rowStartPosition + Vector3.right * i * tileSpacing;

            // Instantiate tile prefab
            GameObject tileObject = Instantiate(_tilePrefab, tilePosition, Quaternion.identity, transform);

            // Assign Holder component to the array and availableHolders list
            Holder tileComponent = tileObject.GetComponent<Holder>();
            if (tileComponent != null)
            {
                waitingTiles[i] = tileComponent;
                availableHolders.Add(tileComponent);
            }
            else
            {
                Debug.LogWarning("Tile prefab does not have a Holder component.");
            }
            tileObject.name = $"Holder [{i}]";
        }
    }

    /// <summary>
    /// Finds the first available (empty) tile in the waiting row.
    /// </summary>
    /// <returns>The first empty Tile, or null if none are available.</returns>
    public Holder FindEmptyTile()
    {
        foreach (var tile in waitingTiles)
        {
            if (!tile.IsOccupied) // Check if the tile is empty
            {
                return tile;
            }
        }
        return null;
    }

    public Holder MoveToNearestAvailableHolder(Stickman stickman)
    {
        foreach (Holder holder in availableHolders)
        {
            if (!holder.IsOccupied)
            {
              //  MoveToHolder(holder.transform.position, 0.5f, Ease.OutCubic);
                return holder;
            }
        }

        Debug.LogWarning("No available holder found.");
        return null;
    }

    public void MoveToHolder(Vector3 targetPosition, float duration, Ease ease)
    {
        transform.DOMove(targetPosition, duration).SetEase(ease);
    }

    /// <summary>
    /// Places a Stickman on the first available Holder in the waiting row.
    /// </summary>
    /// <param name="stickman">The Stickman to place on the Holder.</param>
    /// <returns>True if placement is successful, false otherwise.</returns>
    public bool PlaceStickmanInWaitingRow(Stickman stickman)
    {
        Holder emptyTile = FindEmptyTile();
        if (emptyTile != null)
        {
            emptyTile.AssignStickman(stickman);
            return true;
        }

        Debug.LogWarning("No empty Holders available for the Stickman.");
        return false;
    }

    /// <summary>
    /// Resets the waiting row by clearing all tiles.
    /// </summary>
    public void ResetWaitingRow()
    {
        foreach (var tile in waitingTiles)
        {
            tile.Vacate();
            availableHolders.Clear();
        }
    }
}