using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class Tile : MonoBehaviour
    {
        #region Properties

        // The stickman currently occupying this tile, read-only externally
        public Stickman CurrentStickman => _currentStickman;

        // The world position of the tile
        public Vector3 Position { get; private set; }

        #endregion

        #region Fields

        // Private reference to the current stickman occupying this tile
        [SerializeField]
        private Stickman _currentStickman;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the tile with a given position.
        /// Initially, there is no stickman on this tile.
        /// </summary>
        /// <param name="position">World position of the tile.</param>
        public void Initialize(Vector3 position)
        {
            Position = position;
            _currentStickman = null;  // Initially, no stickman on this tile
        }

        /// <summary>
        /// Attempts to place a stickman on this tile.
        /// If the tile is already occupied, this will fail.
        /// </summary>
        /// <param name="stickman">The stickman to place on this tile.</param>
        /// <returns>True if the stickman was placed successfully, false if the tile is occupied.</returns>
        public bool PlaceStickman(Stickman stickman)
        {
            if (HasStickman())
            {
                Debug.LogWarning($"Cannot place Stickman on tile at {Position} because it's already occupied.");
                return false;  // Tile is already occupied
            }

            _currentStickman = stickman;
            Debug.Log($"Stickman placed on tile at position: {Position}");
            return true;
        }

        /// <summary>
        /// Removes the stickman from this tile.
        /// </summary>
        /// <returns>True if a stickman was removed, false if the tile was empty.</returns>
        public bool RemoveStickman()
        {
            if (!HasStickman())
            {
                Debug.LogWarning($"No Stickman to remove from tile at {Position}.");
                return false;  // No stickman to remove
            }

            _currentStickman = null;  // Remove the stickman
            Debug.Log($"Stickman removed from tile at position: {Position}");
            return true;
        }

        /// <summary>
        /// Checks if there is a stickman currently occupying this tile.
        /// </summary>
        /// <returns>True if there is a stickman, otherwise false.</returns>
        public bool HasStickman()
        {
            return _currentStickman != null;
        }

        #endregion
    }
}
