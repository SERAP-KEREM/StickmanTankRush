using _Main._Stickman;
using _Main._Stickman.StickmanGrid;
using UnityEngine;

namespace _Main
{
    /// <summary>
    /// Represents a holder where a Stickman can be assigned.
    /// </summary>
    public class Holder : BaseOccupiable
    {
      
        #region Fields
        [SerializeField] private Transform _stickmanPoint;
        #endregion

        #region Public Methods
        public Vector3 GetStickmanTargetPosition()
        {
            return _stickmanPoint != null ? _stickmanPoint.position : transform.position;
        }
        public override bool AssignStickman(Stickman stickman)
        {
            // Önce yol kontrolü yap
            var gridPathFinder = FindObjectOfType<GridPathFinder>();
            if (gridPathFinder != null && !gridPathFinder.HasValidPathToTarget(stickman))
            {
                Debug.Log($"[Holder] No valid path for stickman to holder {name}");
                return false;
            }

            // Base class kontrolü
            if (!base.AssignStickman(stickman))
            {
                Debug.LogWarning($"[Holder] Base AssignStickman failed for {name}");
                return false;
            }

            // Stickman'i hareket ettir
            Vector3 targetPos = GetStickmanTargetPosition();
            targetPos.y = stickman.transform.position.y;
            stickman.MoveToHolder(targetPos);

            Debug.Log($"[Holder] Successfully assigned stickman to {name}");
            return true;
        }

        public override Stickman RemoveStickman()
        {
            if (!IsOccupied)
            {
                Debug.LogWarning($"[Holder] {name} is already empty!");
                return null;
            }

            var stickman = base.RemoveStickman();
            Debug.Log($"[Holder] Removed stickman from {name}");
            return stickman;
        }
        #endregion
    }
}
