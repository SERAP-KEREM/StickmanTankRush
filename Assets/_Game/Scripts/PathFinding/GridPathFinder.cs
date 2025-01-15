using _Main._Tank;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridPathFinder : MonoBehaviour
{
    private TileGrid _tileGrid;
    private List<Vector2Int> _currentPath;
    private bool _isInitialized;

    [Header("Debug")]
    [SerializeField] private bool _showDebug = true;

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

    public bool HasValidPathToTarget(Stickman stickman)
    {
        if (!ValidateState(stickman)) return false;

        _currentPath = new List<Vector2Int>();
        Vector2Int start = new Vector2Int(stickman.GridX, stickman.GridY);

        // z=0 kontrolü
        if (start.y == 0)
        {
            Debug.Log("[GridPathFinder] Direct access (z=0)");
            return true;
        }

        // Ba?lang?ç tile'?n? ekle
        _currentPath.Add(start);

        // Yol ara
        bool hasPath = FindValidPath(start);
        LogPathResult(start, hasPath);
        return hasPath;
    }

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

    private bool FindValidPath(Vector2Int currentPos)
    {
        // Kom?ular? kontrol et
        var neighbors = GetEmptyNeighbors(currentPos);
        if (neighbors == null || neighbors.Count == 0)
        {
            if (_showDebug)
                Debug.Log($"[GridPathFinder] No empty neighbors at [{currentPos.x},{currentPos.y}]");
            return false;
        }

        foreach (var neighbor in neighbors)
        {
            // z=0'a ula?t?k m??
            if (neighbor.y == 0)
            {
                _currentPath.Add(neighbor);
                return true;
            }

            // Bu kom?u daha önce ziyaret edilmi? mi?
            if (_currentPath.Contains(neighbor))
                continue;

            // Yeni yolu dene
            _currentPath.Add(neighbor);
            if (FindValidPath(neighbor))
                return true;

            // Bu yol ç?kmaz ise geri al
            _currentPath.RemoveAt(_currentPath.Count - 1);
        }

        return false;
    }

    private List<Vector2Int> GetEmptyNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>();
        var directions = new Vector2Int[]
        {
            new Vector2Int(0, -1),  // ön
            new Vector2Int(1, 0),   // sa?
            new Vector2Int(-1, 0),  // sol
            new Vector2Int(0, 1)    // arka
        };

        foreach (var dir in directions)
        {
            var checkPos = pos + dir;
            if (!IsValidPosition(checkPos))
                continue;

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
        if (!_isInitialized || _tileGrid == null) return false;

        return pos.x >= 0 && pos.x < _tileGrid.GridSize.x &&
               pos.y >= 0 && pos.y < _tileGrid.GridSize.y;
    }

    public List<Vector3> GetPathPositions()
    {
        if (!_isInitialized || _currentPath == null || _currentPath.Count == 0)
            return null;

        return _currentPath.Select(p => new Vector3(p.x, 0, p.y)).ToList();
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
}