using _Main._Stickman.StickmanGrid;
using UnityEngine;

namespace _Input
{
    public class InputHandler : MonoBehaviour
    {
        #region Singleton
        public static InputHandler Instance;

        #endregion

        #region Events
        public delegate void StickmanSelectedEvent(Stickman stickman);
        public event StickmanSelectedEvent OnStickmanSelected;

        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Input Checking
        public void CheckInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button click
            {
                HandleStickmanSelection();
            }
        }

        private void HandleStickmanSelection()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  // Ray from mouse click
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Stickman clickedStickman = hit.collider.GetComponent<Stickman>();
                if (clickedStickman != null && clickedStickman.IsSelectable)
                {
                    OnStickmanSelected?.Invoke(clickedStickman); // Trigger event if Stickman is selected
                }
            }
        }
        #endregion
    }
}
