using UnityEngine;

namespace _Main._Stickman.StickmanGrid
{
    public class Tile : MonoBehaviour
    {
        public Stickman CurrentStickman { get; private set; }  // Bu tile'daki stickman
        public Vector3 Position { get; private set; }  // Tile'?n dünya pozisyonu

        public void Initialize(Vector3 position)
        {
            Position = position;
            CurrentStickman = null;  // Ba?lang?çta bo?
        }

        // Tile'a bir stickman yerle?tir
        public void PlaceStickman(Stickman stickman)
        {
            CurrentStickman = stickman;
        }

        // Tile'dan stickman'? kald?r
        public void RemoveStickman()
        {
            CurrentStickman = null;
        }

        // Tile'da stickman var m??
        public bool HasStickman()
        {
            return CurrentStickman != null;
        }
    }
}
