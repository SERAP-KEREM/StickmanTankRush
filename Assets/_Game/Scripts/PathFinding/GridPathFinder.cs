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
    private TileGrid _tileGrid;
    private List<Vector2Int> _currentPath;
    private bool _isInitialized;

    private readonly Vector2Int[] _directions = new Vector2Int[]
    {
        new Vector2Int(0, -1),  // Forward
        new Vector2Int(1, 0),   // Right
        new Vector2Int(-1, 0),  // Left
        new Vector2Int(0, 1)    // Back
    };
    #endregion

    #region Debug Settings
    [Group("Debug")]
    [SerializeField]
    [PropertyTooltip("Enable debug logging")]
    private bool _showDebug = true;
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the pathfinder with a tile grid reference.
    /// </summary>
    /// <param name="tileGrid">The tile grid to perform pathfinding on.</param>
    public void Initialize(TileGrid tileGrid)
    {
        if (tileGrid == null)
        {
            Debug.LogError("[GridPathFinder] Cannot initialize with null TileGrid!");
            return;
        }

        _tileGrid = tileGrid;
        _isInitialized = true;
        Debug.Log("[GridPathFinder] Initialized successfully");
    }

    /// <summary>
    /// Checks if there is a valid path from the stickman's position to the target (z=0).
    /// </summary>
    /// <param name="stickman">The stickman to find a path for.</param>
    /// <returns>True if a valid path exists, false otherwise.</returns>
    public bool HasValidPathToTarget(Stickman stickman)
    {
        if (!ValidateState(stickman)) return false;

        _currentPath = new List<Vector2Int>();
        Vector2Int start = new Vector2Int(stickman.GridX, stickman.GridY);

        if (IsDirectAccess(start))
        {
            return true;
        }

        _currentPath.Add(start);
        bool hasPath = FindValidPath(start);
        LogPathResult(start, hasPath);
        return hasPath;
    }

    /// <summary>
    /// Gets the world positions of the current path.
    /// </summary>
    /// <returns>List of Vector3 positions representing the path, or null if no path exists.</returns>
    public List<Vector3> GetPathPositions()
    {
        if (!ValidatePathExists()) return null;

        return _currentPath.Select(p => new Vector3(p.x, 0, p.y)).ToList();
    }
    #endregion

    #region Private Methods
    private bool ValidateState(Stickman stickman)
    {
        if (!_isInitialized)
        {
            Debug.LogError("[GridPathFinder] Not initialized!");
            return false;
        }

        if (_tileGrid == null)
        {
            Debug.LogError("[GridPathFinder] TileGrid is null!");
            return false;
        }

        if (stickman == null)
        {
            Debug.LogError("[GridPathFinder] Stickman is null!");
            return false;
        }

        return true;
    }

    private bool ValidatePathExists()
    {
        return _isInitialized && _currentPath != null && _currentPath.Count > 0;
    }

    private bool IsDirectAccess(Vector2Int position)
    {
        if (position.y == 0)
        {
            Debug.Log("[GridPathFinder] Direct access (z=0)");
            return true;
        }
        return false;
    }

    private bool FindValidPath(Vector2Int currentPos)
    {
        var neighbors = GetEmptyNeighbors(currentPos);
        if (!ValidateNeighbors(neighbors, currentPos))
        {
            return false;
        }

        foreach (var neighbor in neighbors)
        {
            if (IsTargetReached(neighbor))
            {
                _currentPath.Add(neighbor);
                return true;
            }

            if (_currentPath.Contains(neighbor))
            {
                continue;
            }

            if (TryPathThroughNeighbor(neighbor))
            {
                return true;
            }
        }

        return false;
    }

    private bool ValidateNeighbors(List<Vector2Int> neighbors, Vector2Int pos)
    {
        if (neighbors == null || neighbors.Count == 0)
        {
            if (_showDebug)
            {
                Debug.Log($"[GridPathFinder] No empty neighbors at [{pos.x},{pos.y}]");
            }
            return false;
        }
        return true;
    }

    private bool IsTargetReached(Vector2Int position)
    {
        return position.y == 0;
    }

    private bool TryPathThroughNeighbor(Vector2Int neighbor)
    {
        _currentPath.Add(neighbor);
        if (FindValidPath(neighbor))
        {
            return true;
        }
        _currentPath.RemoveAt(_currentPath.Count - 1);
        return false;
    }

    private List<Vector2Int> GetEmptyNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>();

        foreach (var dir in _directions)
        {
            var checkPos = pos + dir;
            if (!IsValidPosition(checkPos))
            {
                continue;
            }

            var tile = _tileGrid.GetTileAt(checkPos.x, checkPos.y);
            if (tile != null && !tile.IsOccupied)
            {
                neighbors.Add(checkPos);
            }
        }

        return neighbors;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        if (!_isInitialized || _tileGrid == null)
        {
            return false;
        }

        return pos.x >= 0 && pos.x < _tileGrid.GridSize.x &&
               pos.y >= 0 && pos.y < _tileGrid.GridSize.y;
    }

    private void LogPathResult(Vector2Int start, bool hasPath)
    {
        if (!_showDebug) return;

        if (hasPath)
        {
            string path = string.Join(" -> ", _currentPath.Select(p => $"[{p.x},{p.y}]"));
            Debug.Log($"[GridPathFinder] Found path from [{start.x},{start.y}]: {path}");
        }
        else
        {
            Debug.Log($"[GridPathFinder] No valid path from [{start.x},{start.y}]");
        }
    }
    #endregion
}