using UnityEngine;
using _Main._Stickman.StickmanGrid; // Stickman ve StickmanGrid'yi kullanabilmek için


    public class GridNeighborChecker : MonoBehaviour
    {
        private StickmanGrid _stickmanGrid; // StickmanGrid referans?

        // Start method to initialize StickmanGrid
        void Start()
        {
            _stickmanGrid = FindObjectOfType<StickmanGrid>();
        }

        // Kom?ular? kontrol etmek için bir metod
        public void CheckNeighbors(Stickman clickedStickman)
        {
            // Stickman'?n bulundu?u grid pozisyonunu al?yoruz
            int gridX = clickedStickman.GridX;
            int gridY = clickedStickman.GridY;

            // Debug log ile hangi gridin üzerindeyse yazd?ral?m
            Debug.Log($"Clicked Stickman is at grid position [{gridX}, {gridY}]");

            // Kom?u gridlerin kontrolü
            CheckGridNeighbor(gridX - 1, gridY, "Left");
            CheckGridNeighbor(gridX + 1, gridY, "Right");
            CheckGridNeighbor(gridX, gridY - 1, "Down");
            CheckGridNeighbor(gridX, gridY + 1, "Up");
        }

        // Grid pozisyonunun içinde stickman olup olmad???n? kontrol eden metod
        private void CheckGridNeighbor(int x, int y, string direction)
        {
            if (_stickmanGrid == null) return;

            // E?er pozisyon d??ar?da ise, kom?u yok demek
            if (x < 0 || x >= _stickmanGrid.GridSize.x || y < 0 || y >= _stickmanGrid.GridSize.y)
            {
                Debug.Log($"{direction} neighbor grid [{x}, {y}] is outside the grid bounds.");
                return;
            }

            // E?er kom?u gridde bir Stickman varsa
            Stickman neighbor = _stickmanGrid.GetStickmanAtPosition(x, y);

            if (neighbor != null)
            {
                Debug.Log($"{direction} neighbor grid [{x}, {y}] is occupied by Stickman.");
            }
            else
            {
                Debug.Log($"{direction} neighbor grid [{x}, {y}] is empty.");
            }
        }
    }

