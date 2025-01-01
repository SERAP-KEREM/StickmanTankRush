using _Main._Stickman.StickmanGrid;
using UnityEngine;
using UnityEngine.Events;

namespace _Input
{
    public class InputHandler : MonoBehaviour
    {
        #region Singleton
        public static InputHandler Instance;

        #endregion

        #region Events
        /// <summary>
        /// Event triggered when the screen is touched or clicked.
        /// </summary>
        public event UnityAction<Vector3> OnStickmanSelected;
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
        private void Update()
        {
            HandleStickmanSelection();
        }
        #endregion

        #region Input Checking


        private void HandleStickmanSelection()
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                Vector3 mousePosition = Input.mousePosition;
                OnStickmanSelected?.Invoke(mousePosition);
            }

        }
        #endregion
    }
}
