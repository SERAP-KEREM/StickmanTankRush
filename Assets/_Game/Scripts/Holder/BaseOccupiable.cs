using _Main._Stickman.StickmanGrid;
using UnityEngine;

namespace _Main._Stickman
{
    /// <summary>
    /// Base class for objects that can be occupied by a Stickman.
    /// </summary>
    public abstract class BaseOccupiable : MonoBehaviour
    {
        #region Fields

        [SerializeField, Tooltip("The Stickman currently occupying this object.")]
        private Stickman _currentStickman; // Private field for the assigned stickman
        #endregion

        #region Properties

        /// <summary>
        /// The Stickman currently occupying this object.
        /// </summary>
        public Stickman CurrentStickman
        {
            get => _currentStickman;
            protected set => _currentStickman = value;
        }

        /// <summary>
        /// Indicates whether this object is currently occupied by a Stickman.
        /// </summary>
        public bool IsOccupied => _currentStickman != null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Assigns a Stickman to this object.
        /// </summary>
        /// <param name="stickman">The Stickman to assign.</param>
        /// <returns>True if the Stickman was successfully assigned; false otherwise.</returns>
        public virtual bool AssignStickman(Stickman stickman)
        {
            if (stickman == null)
            {
                Debug.LogError("Cannot assign a null Stickman.");
                return false;
            }

            if (IsOccupied)
            {
                Debug.LogWarning($"This object is already occupied by Stickman '{_currentStickman.name}'. Stickman will be replaced.");
                RemoveStickman();
            }

            CurrentStickman = stickman;
           // Debug.Log($"Stickman '{stickman.name}' successfully assigned to {name}.");
            return true;
        }

        /// <summary>
        /// Removes the Stickman from this object.
        /// </summary>
        /// <returns>The removed Stickman, or null if no Stickman was assigned.</returns>
        public virtual Stickman RemoveStickman()
        {
            if (!IsOccupied)
            {
                Debug.LogWarning("No Stickman to remove.");
                return null;
            }

            Stickman removedStickman = CurrentStickman;
            CurrentStickman = null;
            //Debug.Log($"Stickman '{removedStickman.name}' removed from {name}.");
            return removedStickman;
        }

        #endregion
    }
}

