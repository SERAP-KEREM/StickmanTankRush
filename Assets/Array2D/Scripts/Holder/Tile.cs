using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class Tile : MonoBehaviour
    {
        // The stickman currently occupying this tile
        public Stickman CurrentStickman => _currentStickman;

        // The world position of the tile
        public Vector3 Position { get; private set; }

        // Private reference to the current stickman occupying this tile
        [SerializeField]
        private Stickman _currentStickman;

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
        /// Places a stickman on this tile.
        /// </summary>
        /// <param name="stickman">The stickman to place on this tile.</param>
        public void PlaceStickman(Stickman stickman)
        {
            if (_currentStickman != null)
            {
                Debug.LogWarning("This tile is already occupied.");
                return;  // If there's already a stickman, do not place another
            }

            _currentStickman = stickman;  // Set the stickman on the tile
           
            Debug.Log($"Stickman placed on tile at position: {Position}");
        }

        /// <summary>
        /// Removes the stickman from this tile.
        /// </summary>
        public void RemoveStickman()
        {
            if (_currentStickman == null)
            {
                Debug.LogWarning("There is no stickman to remove.");
                return;  // If there is no stickman, do nothing
            }

            _currentStickman = null;  // Remove the stickman
            Debug.Log($"Stickman removed from tile at position: {Position}");
        }

        /// <summary>
        /// Checks if there is a stickman on this tile.
        /// </summary>
        /// <returns>True if there is a stickman, otherwise false.</returns>
        public bool HasStickman()
        {
            return _currentStickman != null;  // Return true if the tile has a stickman
        }
    }
}
