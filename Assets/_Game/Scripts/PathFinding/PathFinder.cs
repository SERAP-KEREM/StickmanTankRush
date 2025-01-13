using UnityEngine;
using UnityEngine.AI;

namespace _Main._Stickman.PathSystem
{
    public class PathFinder : MonoBehaviour
    {
        #region Singleton
        private static PathFinder _instance;

        [Header("Debug")]
        [SerializeField] private bool _showPaths = true;
        [SerializeField] private Color _validPathColor = Color.green;
        [SerializeField] private Color _invalidPathColor = Color.red;
        public static PathFinder Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PathFinder>();
                    if (_instance == null)
                    {
                        var go = new GameObject("PathFinder");
                        _instance = go.AddComponent<PathFinder>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        public bool CanReachTarget(Vector3 start, Vector3 end)
        {
            NavMeshPath path = new NavMeshPath();
            return NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);
        }

        public NavMeshPath GetPath(Vector3 start, Vector3 end)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
            {
                return path;
            }
            return null;
        }
        public bool IsPathValid(Vector3 start, Vector3 end)
        {
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

            if (_showPaths)
            {
                Gizmos.color = hasPath ? _validPathColor : _invalidPathColor;
                if (path.corners.Length > 0)
                {
                    for (int i = 0; i < path.corners.Length - 1; i++)
                    {
                        Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                    }
                }
            }

            return hasPath && path.status == NavMeshPathStatus.PathComplete;
        }
        private void OnDrawGizmos()
        {
            if (!_showPaths || !Application.isPlaying) return;

            // Path visualization can be added here
            // We can visualize active paths if needed
        }
    }
}