using _Main._Stickman;
using _Main._Stickman.StickmanGrid;
using DG.Tweening;
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
            Vector3 holderPos = _stickmanPoint != null ? _stickmanPoint.position : transform.position;
            return new Vector3(holderPos.x, 0f, holderPos.z); // Holder'da y=0
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

            Vector3 targetPos = GetStickmanTargetPosition();
            Vector3 startPos = stickman.transform.position;
            Vector3 midPoint = (startPos + targetPos) * 0.5f + Vector3.up * 5f;

            stickman.transform.DOPath(
                new Vector3[] { startPos, midPoint, targetPos },
                1f,
                PathType.CatmullRom
            ).SetEase(Ease.InOutQuad);

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
