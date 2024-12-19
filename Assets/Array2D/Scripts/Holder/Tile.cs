using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class Tile : MonoBehaviour
    {
        public Stickman CurrentStickman { get; private set; }  // The stickman currently on this tile
        public Vector3 Position { get; private set; }  // The world position of the tile

        /// <summary>
        /// Initializes the tile with a given position.
        /// </summary>
        public void Initialize(Vector3 position)
        {
            Position = position;
            CurrentStickman = null;  // Initially, no stickman
        }

        /// <summary>
        /// Places a stickman on this tile.
        /// </summary>
        public void PlaceStickman(Stickman stickman)
        {
            CurrentStickman = stickman;
        }

        /// <summary>
        /// Removes the stickman from this tile.
        /// </summary>
        public void RemoveStickman()
        {
            CurrentStickman = null;
        }

        /// <summary>
        /// Checks if there is a stickman on this tile.
        /// </summary>
        public bool HasStickman()
        {
            return CurrentStickman != null;
        }
    }
}
