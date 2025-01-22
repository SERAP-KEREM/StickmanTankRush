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
    private TileGrid _tileGrid;
    private List<Vector2Int> _currentPath;
    private Vector2Int[] _directions = new Vector2Int[]
    {
        new Vector2Int(0, -1),  // Ön
        new Vector2Int(1, 0),   // Sa?
        new Vector2Int(-1, 0),  // Sol
        new Vector2Int(0, 1)    // Arka
    };

    private void Awake()
    {
        _tileGrid = FindObjectOfType<TileGrid>();
        _currentPath = new List<Vector2Int>();
    }

    public bool HasValidPathToTarget(Stickman stickman)
    {
        if (stickman == null) return false;

        // En öndeki (y=0) veya holder'daki stickmanlar direkt hareket edebilir
        if (stickman.GridY == 0 || stickman.IsInHolder)
        {
            return true;
        }
        
        _currentPath.Clear();
        Vector2Int startPos = new Vector2Int(stickman.GridX, stickman.GridY);
        _currentPath.Add(startPos);

        return FindPath(startPos);
    }

    private bool FindPath(Vector2Int currentPos)
    {
        // y=0'a ula?t?ysak yolu bulduk demektir
        if (currentPos.y == 0)
        {
            return true;
        }

        // Kom?ular? kontrol et (ön, sa?, sol, arka s?ras?yla)
        foreach (Vector2Int direction in _directions)
        {
            Vector2Int nextPos = currentPos + direction;

            // Bu pozisyonu daha önce kulland?k m??
            if (_currentPath.Contains(nextPos)) continue;

            // Pozisyon grid içinde mi ve tile bo? mu?
            if (IsValidTile(nextPos))
            {
                _currentPath.Add(nextPos);

                if (FindPath(nextPos))
                {
                    return true;
                }

                // Bu yol ç?kmaza girdi, son eklenen pozisyonu ç?kar
                _currentPath.RemoveAt(_currentPath.Count - 1);
            }
        }

        return false;
    }

    private bool IsValidTile(Vector2Int pos)
    {
        // Grid s?n?rlar? içinde mi?
        if (pos.x < 0 || pos.x >= _tileGrid.GridSize.x ||
            pos.y < 0 || pos.y >= _tileGrid.GridSize.y)
        {
            return false;
        }
        var tile = _tileGrid.GetTileAt(pos.x, pos.y);
        return tile != null && !tile.IsOccupied;
    }

    public List<Vector3> GetPathPoints()
    {
        List<Vector3> pathPoints = new List<Vector3>();

        foreach (Vector2Int point in _currentPath)
        {
            // Grid pozisyonunu world pozisyonuna çevir
            pathPoints.Add(new Vector3(point.x, 0, point.y));
        }

        return pathPoints;
    }
}