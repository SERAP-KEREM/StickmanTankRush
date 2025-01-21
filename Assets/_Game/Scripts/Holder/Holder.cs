using UnityEngine;
using _Main._Stickman;
using _Main._Stickman.StickmanGrid;
using TriInspector;

namespace _Main
{
    /// <summary>
    /// Represents a holder that can temporarily store a stickman.
    /// Inherits from BaseOccupiable to handle basic occupancy functionality.
    /// </summary>
    [DeclareHorizontalGroup("Holder")]
    public class Holder : BaseOccupiable
    {
        #region Inspector Fields
        [SerializeField]
        [Required]
        [PropertyTooltip("Transform point where the stickman should be positioned")]
        private Transform _stickmanPoint;
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the target position where a stickman should be placed.
        /// </summary>
        /// <returns>The world position where the stickman should move to.</returns>
        public Vector3 GetStickmanTargetPosition()
        {
            return _stickmanPoint != null
                ? _stickmanPoint.position
                : transform.position;
        }

        /// <summary>
        /// Assigns a stickman to this holder and moves it to the correct position.
        /// </summary>
        /// <param name="stickman">The stickman to assign to this holder.</param>
        /// <returns>True if the assignment was successful, false otherwise.</returns>
        public override bool AssignStickman(Stickman stickman)
        {
            if (!ValidateAssignment(stickman)) return false;

            if (!base.AssignStickman(stickman)) return false;
            stickman.IsInHolder = true;
            MoveStickmanToPosition(stickman);
            return true;
        }

        /// <summary>
        /// Removes and returns the currently assigned stickman.
        /// </summary>
        /// <returns>The removed stickman, or null if the holder is empty.</returns>
        public override Stickman RemoveStickman()
        {
            if (!IsOccupied)
            {
                LogWarningEmpty();
                return null;
            }
        
            var stickman = base.RemoveStickman();
            stickman.IsInHolder = false;
            LogStickmanRemoval();
            return stickman;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validates whether a stickman can be assigned to this holder.
        /// </summary>
        private bool ValidateAssignment(Stickman stickman)
        {
            if (stickman == null)
            {
                Debug.LogWarning("[Holder] Cannot assign null stickman.");
                return false;
            }

            if (IsOccupied)
            {
                Debug.LogWarning("[Holder] Holder is already occupied.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Moves the assigned stickman to the holder's position.
        /// </summary>
        private void MoveStickmanToPosition(Stickman stickman)
        {
            Vector3 targetPos = GetStickmanTargetPosition();
            stickman.MoveToHolder(targetPos);
        }

        private void LogWarningEmpty()
        {
            Debug.LogWarning($"[Holder] {name} is already empty!");
        }

        private void LogStickmanRemoval()
        {
            Debug.Log($"[Holder] Removed stickman from {name}");
        }
        #endregion
    }
}