using _Main._Stickman.StickmanGrid;
using UnityEngine;

namespace _Main
{
    /// <summary>
    /// Represents a holder where a Stickman can be assigned.
    /// Handles the state of occupancy and manages Stickman movement to the holder.
    /// </summary>
    public class Holder : MonoBehaviour
    {
        #region Fields

        [Header("Holder State")]
        [Tooltip("Currently assigned Stickman.")]
        [SerializeField]
        private Stickman assignedStickman;

        #endregion

        #region Properties

        /// <summary>
        /// Returns whether this Holder is currently occupied by a Stickman.
        /// </summary>
        public bool IsOccupied => assignedStickman != null;

        #endregion

        #region Methods

        /// <summary>
        /// Assigns a Stickman to this Holder.
        /// Moves the Stickman to the Holder's position while preserving its current Y position.
        /// </summary>
        /// <param name="stickman">The Stickman to assign.</param>
        public void AssignStickman(Stickman stickman)
        {
            if (stickman == null)
            {
                Debug.LogError("Cannot assign a null Stickman to Holder.");
                return;
            }

            // Assign the Stickman to this holder
            assignedStickman = stickman;

            // Move Stickman to the Holder's position (keeping the Y position intact)
            MoveStickmanToHolder(stickman);

            Debug.Log($"Stickman '{stickman.name}' assigned to Holder '{name}'.");
        }

        /// <summary>
        /// Moves the Stickman to the Holder's position, keeping the Y-coordinate unchanged.
        /// </summary>
        /// <param name="stickman">The Stickman to move.</param>
        private void MoveStickmanToHolder(Stickman stickman)
        {
            Vector3 targetPosition = transform.position; // Holder's position
            targetPosition.y = stickman.transform.position.y; // Keep the Stickman's current Y position

            // Move the Stickman to the target position
            stickman.transform.position = targetPosition;
        }
        public void PlaceStickman(Stickman stickman)
        {
            assignedStickman = stickman;
        }
        public Stickman GetStickman()
        {
            return assignedStickman;
        }
        // Holder'dan stickman'? ç?kar?r
        public void RemoveStickman()
        {
            if (assignedStickman != null)
            {
                assignedStickman = null;  // Stickman'? bo?alt?yoruz
            }
        }
        #endregion
    }
}
