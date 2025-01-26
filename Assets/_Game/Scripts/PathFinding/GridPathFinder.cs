using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TriInspector;

/// <summary>
/// Handles pathfinding operations on a grid-based system.
/// Finds valid paths for stickmen movement considering obstacles and grid boundaries.
/// </summary>
[DeclareBoxGroup("Debug")]
public class GridPathFinder : MonoBehaviour
{
    #region Private Fields
    /// <summary>
    /// Reference to the TileGrid, which contains the grid data.
    /// </summary>
    private TileGrid _tileGrid;

    /// <summary>
    /// Current path being calculated for the stickman.
    /// </summary>
    private List<Vector2Int> _currentPath;

    /// <summary>
    /// Directions for moving in the grid (up, right, left, down).
    /// </summary>
    private Vector2Int[] _directions = new Vector2Int[]
    {
        new Vector2Int(0, -1),  // Up
        new Vector2Int(1, 0),   // Right
        new Vector2Int(-1, 0),  // Left
        new Vector2Int(0, 1)    // Down
    };
    #endregion

    #region Initialization
    /// <summary>
    /// Initializes the GridPathFinder and assigns the TileGrid reference.
    /// </summary>
    private void Awake()
    {
        _tileGrid = FindObjectOfType<TileGrid>();
        _currentPath = new List<Vector2Int>();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Checks if there is a valid path for the given stickman to the target (usually the tank).
    /// </summary>
    /// <param name="stickman">The stickman for which the pathfinding is being checked.</param>
    /// <returns>Returns true if there is a valid path, false otherwise.</returns>
    public bool HasValidPathToTarget(Stickman stickman)
    {
        if (stickman == null) return false;

        if (stickman.GridY == 0 || stickman.IsInHolder)
        {
            return true;
        }

        _currentPath.Clear();
        Vector2Int startPos = new Vector2Int(stickman.GridX, stickman.GridY);
        _currentPath.Add(startPos);

        return FindPath(startPos);
    }
    #endregion

    #region Pathfinding Logic
    /// <summary>
    /// Recursively finds a valid path from the current position to the target (usually at y=0).
    /// </summary>
    /// <param name="currentPos">The current position of the stickman on the grid.</param>
    /// <returns>Returns true if a valid path is found, false otherwise.</returns>
    private bool FindPath(Vector2Int currentPos)
    {
        // Base case: If we've reached the target (y = 0), return true.
        if (currentPos.y == 0)
        {
            return true;
        }

        foreach (Vector2Int direction in _directions)
        {
            Vector2Int nextPos = currentPos + direction;

            if (_currentPath.Contains(nextPos)) continue;

            if (IsValidTile(nextPos))
            {
                _currentPath.Add(nextPos);

                if (FindPath(nextPos))
                {
                    return true;
                }

                _currentPath.RemoveAt(_currentPath.Count - 1);
            }
        }

        return false;
    }
    #endregion

    #region Validation Methods
    /// <summary>
    /// Checks if a given position is a valid tile to move to.
    /// </summary>
    /// <param name="pos">The position to check.</param>
    /// <returns>Returns true if the tile is valid (inside the grid and not occupied), false otherwise.</returns>
    private bool IsValidTile(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= _tileGrid.GridSize.x ||
            pos.y < 0 || pos.y >= _tileGrid.GridSize.y)
        {
            return false;
        }

        var tile = _tileGrid.GetTileAt(pos.x, pos.y);
        return tile != null && !tile.IsOccupied;
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Converts the current path into world space points for visualization or movement.
    /// </summary>
    /// <returns>A list of Vector3 points representing the path.</returns>
    public List<Vector3> GetPathPoints()
    {
        List<Vector3> pathPoints = new List<Vector3>();

        foreach (Vector2Int point in _currentPath)
        {
            pathPoints.Add(new Vector3(point.x, 0, point.y));
        }

        return pathPoints;
    }
    #endregion
}

