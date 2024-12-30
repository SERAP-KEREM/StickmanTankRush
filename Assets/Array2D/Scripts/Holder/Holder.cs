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
        #region Public Methods

        /// <summary>
        /// Moves the assigned Stickman to the holder's position.
        /// </summary>
        /// <param name="stickman">The Stickman to assign and move.</param>
        /// <returns>True if the Stickman was successfully assigned and moved; false otherwise.</returns>
        public override bool AssignStickman(Stickman stickman)
        {
            // Call the base method to check if assignment is valid
            if (!base.AssignStickman(stickman)) return false;

            // Move Stickman to the Holder's position while preserving its Y position
            stickman.MoveToHolder(new Vector3(transform.position.x, stickman.transform.position.y, transform.position.z));

            Debug.Log($"Stickman '{stickman.name}' successfully assigned and moved to Holder '{name}'.");
            return true;
        }

        #endregion
    }
}
