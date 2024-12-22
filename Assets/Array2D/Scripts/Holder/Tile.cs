using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class Tile : MonoBehaviour
    {
        public Stickman CurrentStickman { get => _currentStickman; }  // The stickman currently on this tile
        public Vector3 Position { get; private set; }  // The world position of the tile

        [SerializeField]
        private Stickman _currentStickman; 
        /// <summary>
        /// Initializes the tile with a given position.
        /// </summary>
        public void Initialize(Vector3 position)
        {
            Position = position;
            _currentStickman = null;  // Initially, no stickman
        }

        /// <summary>
        /// Places a stickman on this tile.
        /// </summary>
        public void PlaceStickman(Stickman stickman)
        {
            _currentStickman = stickman;
        }

        /// <summary>
        /// Removes the stickman from this tile.
        /// </summary>
        public void RemoveStickman()
        {
            _currentStickman = null;
        }

        /// <summary>
        /// Checks if there is a stickman on this tile.
        /// </summary>
        public bool HasStickman()
        {
            return _currentStickman != null;
        }
    }
}
