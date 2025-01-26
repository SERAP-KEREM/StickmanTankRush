using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    /// <summary>
    /// Represents a tile within the Stickman grid system.
    /// A tile can hold a Stickman and provides information about its position and state.
    /// </summary>
    public class Tile : BaseOccupiable
    {
        #region Fields

        [Header("Tile Configuration")]
        [Tooltip("The world position of the tile.")]
        [SerializeField, HideInInspector]
        private Vector3 _position;

        #endregion

        #region Pathfinding Properties

        /// <summary>
        /// The X coordinate of the tile in the grid.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y coordinate of the tile in the grid.
        /// </summary>
        public int Y { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// The world position of the tile in the grid.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            private set => _position = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the tile with a given position and resets its state.
        /// </summary>
        /// <param name="position">The world position of the tile.</param>
        public void Initialize(Vector3 position)
        {
            Position = position;
            X = Mathf.RoundToInt(position.x);
            Y = Mathf.RoundToInt(position.z);
            CurrentStickman = null; // Reset the Stickman assigned to this tile
        }

        /// <summary>
        /// Assigns a Stickman to this tile.
        /// </summary>
        /// <param name="stickman">The Stickman to assign.</param>
        /// <returns>True if the Stickman was successfully assigned; otherwise, false.</returns>
        public override bool AssignStickman(Stickman stickman)
        {
            bool success = base.AssignStickman(stickman);
            return success;
        }

        /// <summary>
        /// Removes the Stickman assigned to this tile.
        /// </summary>
        /// <returns>The Stickman that was removed, or null if no Stickman was assigned.</returns>
        public override Stickman RemoveStickman()
        {
            var stickman = base.RemoveStickman();
            return stickman;
        }

        #endregion
    }
}
