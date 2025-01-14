using UnityEngine;
using System.Collections.Generic;
using _Main._Stickman.StickmanGrid;
using _Main._Tank;
using System.Linq;

namespace _Main._Stickman.PathSystem
{

    public class GridPathFinder : MonoBehaviour
    {
        private TileGrid _tileGrid;
        private const int MOVE_STRAIGHT_COST = 10;
        [Header("Debug")]
        [SerializeField] private bool _showDebug = true;
        [SerializeField] private Color _pathColor = Color.green;
        [SerializeField] private Color _obstacleColor = Color.red;
        [SerializeField] private Color _emptyColor = Color.white;

        private List<Tile> _currentPath;
        public void Initialize(TileGrid tileGrid)
        {
            _tileGrid = tileGrid;
        }
        public List<Vector3> GetPathPositions(Stickman stickman, Vector3 targetWorldPos)
        {
            Vector2Int start = new Vector2Int(stickman.GridX, stickman.GridY);
            Vector2Int target = new Vector2Int(
                Mathf.RoundToInt(targetWorldPos.x),
                Mathf.RoundToInt(targetWorldPos.z)
            );

            List<Tile> path = FindPath(new Vector2(start.x, start.y), new Vector2(target.x, target.y));

            if (path == null) return null;

            return path.Select(t => t.Position).ToList();
        }

        public bool HasValidPathToTank(Stickman stickman, Tank tank)
        {
            if (_tileGrid == null || stickman == null || tank == null)
            {
                Debug.LogWarning("[GridPathFinder] Null reference check failed");
                return false;
            }

            Vector2 startPos = new Vector2(stickman.GridX, stickman.GridY);
            Vector3 tankPos = tank.GetStickmanTargetPosition();
            Vector2 endPos = new Vector2(
                Mathf.RoundToInt(tankPos.x),
                Mathf.RoundToInt(tankPos.z)
            );

            Debug.Log($"[GridPathFinder] Searching path from {startPos} to {endPos}");

            _currentPath = FindPath(startPos, endPos);

            if (_currentPath != null && _currentPath.Count > 0)
            {
                Debug.Log($"[GridPathFinder] Path found with {_currentPath.Count} steps");
                return true;
            }

            Debug.Log("[GridPathFinder] No valid path found");
            return false;
        }
        public bool HasValidPathToPosition(Stickman stickman, Vector2Int targetPos)
        {
            Vector2 start = new Vector2(stickman.GridX, stickman.GridY);
            Vector2 end = new Vector2(targetPos.x, targetPos.y);

            var path = FindPath(start, end);
            return path != null && path.Count > 0;
        }
        private List<Tile> FindPath(Vector2 startPos, Vector2 endPos)
        {
            Tile startTile = _tileGrid.GetTileAt((int)startPos.x, (int)startPos.y);
            Tile endTile = _tileGrid.GetTileAt((int)endPos.x, (int)endPos.y);

            if (startTile == null || endTile == null) return null;

            List<Tile> openList = new List<Tile>();
            List<Tile> closedList = new List<Tile>();

            openList.Add(startTile);

            // Tüm tile'lar? resetle
            for (int x = 0; x < _tileGrid.GridSize.x; x++)
            {
                for (int y = 0; y < _tileGrid.GridSize.y; y++)
                {
                    Tile tile = _tileGrid.GetTileAt(x, y);
                    if (tile != null)
                    {
                        tile.gCost = int.MaxValue;
                        tile.CalculateFCost();
                        tile.parent = null;
                    }
                }
            }

            startTile.gCost = 0;
            startTile.hCost = CalculateDistance(startTile, endTile);
            startTile.CalculateFCost();

            while (openList.Count > 0)
            {
                Tile currentTile = GetLowestFCostTile(openList);

                if (currentTile == endTile)
                {
                    // Hedef tile'a ula?t?k
                    return CalculatePath(endTile);
                }

                openList.Remove(currentTile);
                closedList.Add(currentTile);

                foreach (Tile neighbour in GetNeighbours(currentTile))
                {
                    if (closedList.Contains(neighbour)) continue;
                    if (neighbour.hasObstacle) continue;

                    int tentativeGCost = currentTile.gCost + CalculateDistance(currentTile, neighbour);

                    if (tentativeGCost < neighbour.gCost)
                    {
                        neighbour.parent = currentTile;
                        neighbour.gCost = tentativeGCost;
                        neighbour.hCost = CalculateDistance(neighbour, endTile);
                        neighbour.CalculateFCost();

                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                        }
                    }
                }
            }

            return null;
        }

        private List<Tile> GetNeighbours(Tile tile)
        {
            List<Tile> neighbours = new List<Tile>();

            // Sa?
            if (tile.x + 1 < _tileGrid.GridSize.x)
                neighbours.Add(_tileGrid.GetTileAt(tile.x + 1, tile.y));

            // Sol
            if (tile.x - 1 >= 0)
                neighbours.Add(_tileGrid.GetTileAt(tile.x - 1, tile.y));

            // Yukar?
            if (tile.y + 1 < _tileGrid.GridSize.y)
                neighbours.Add(_tileGrid.GetTileAt(tile.x, tile.y + 1));

            // A?a??
            if (tile.y - 1 >= 0)
                neighbours.Add(_tileGrid.GetTileAt(tile.x, tile.y - 1));

            return neighbours.FindAll(n => n != null);
        }

        private int CalculateDistance(Tile a, Tile b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            return xDistance + yDistance;
        }

        private Tile GetLowestFCostTile(List<Tile> tiles)
        {
            Tile lowestFCostTile = tiles[0];
            for (int i = 1; i < tiles.Count; i++)
            {
                if (tiles[i].fCost < lowestFCostTile.fCost)
                    lowestFCostTile = tiles[i];
            }
            return lowestFCostTile;
        }

        private List<Tile> CalculatePath(Tile endTile)
        {
            List<Tile> path = new List<Tile>();
            Tile currentTile = endTile;

            while (currentTile != null)
            {
                path.Add(currentTile);
                currentTile = currentTile.parent;
            }

            path.Reverse();
            return path;
        }
    }
    public class PathNode
        {
            public Vector2Int Position { get; private set; }
            public PathNode Parent { get; set; }
            public float GCost { get; set; }
            public float HCost { get; private set; }
            public float FCost => GCost + HCost;

            public PathNode(Vector2Int pos, PathNode parent, float gCost, float hCost)
            {
                Position = pos;
                Parent = parent;
                GCost = gCost;
                HCost = hCost;
            }
        }
    }
    public class PathNode
    {
        public Vector2Int Position { get; private set; }
        public PathNode Parent { get; set; }
        public float GCost { get; set; }
        public float HCost { get; private set; }
        public float FCost => GCost + HCost;

        public PathNode(Vector2Int pos, PathNode parent, float gCost, float hCost)
        {
            Position = pos;
            Parent = parent;
            GCost = gCost;
            HCost = hCost;
        }
    }
