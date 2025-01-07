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
        /// Assigns a Stickman to this Holder and moves it to the Holder's position.
        /// </summary>
        /// <param name="stickman">The Stickman to assign and move.</param>
        /// <returns>
        /// True if the Stickman was successfully assigned and moved; 
        /// false if the assignment failed (e.g., Holder is already occupied).
        /// </returns>
        public override bool AssignStickman(Stickman stickman)
        {
            // Call the base method to validate the assignment
            if (!base.AssignStickman(stickman))
                return false;

            // Move Stickman to the Holder's position while preserving its Y position
            stickman.MoveToHolder(new Vector3(
                transform.position.x,
                stickman.transform.position.y,
                transform.position.z
            ));

            // Assignment successful
            return true;
        }

        #endregion
    }
}
