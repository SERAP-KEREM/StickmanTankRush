using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class Tile : BaseOccupiable
    {
        #region Properties

        /// <summary>
        /// The world position of the tile.
        /// </summary>
        public Vector3 Position { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the tile with a given position.
        /// </summary>
        /// <param name="position">World position of the tile.</param>
        public void Initialize(Vector3 position)
        {
            Position = position;
            CurrentStickman = null; // Initially, no Stickman is assigned
        }

        #endregion
    }
}
