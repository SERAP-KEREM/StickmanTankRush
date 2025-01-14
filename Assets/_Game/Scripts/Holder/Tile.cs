using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    /// <summary>
    /// Represents a tile within the Stickman grid system.
    /// A tile can hold a Stickman and provides information about its position.
    /// </summary>
    public class Tile : BaseOccupiable
    {
        #region Fields

        [Header("Tile Configuration")]
        [Tooltip("The world position of the tile.")]
        [SerializeField, HideInInspector] // This can hide it in the Inspector if it should not be manually modified.
        private Vector3 _position;

        #endregion

        #region Pathfinding Properties
        public int gCost;
        public int hCost;
        public int fCost { get; private set; }
        public Tile parent;
        public int x { get; private set; }
        public int y { get; private set; }
        public bool hasObstacle => IsOccupied; // Tile dolu ise engel vard?r
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
            x = Mathf.RoundToInt(position.x);
            y = Mathf.RoundToInt(position.z);
            // Reset the Stickman assigned to this tile
            CurrentStickman = null;
        }
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
        #endregion
    }
}
