using _Main._Stickman.StickmanGrid;
using SerapKeremGameTools._Game._Singleton;
using UnityEngine;
using UnityEngine.Events;

namespace _Input
{
    public class InputHandler : MonoSingleton<InputHandler>
    {
       

        #region Events
        /// <summary>
        /// Event triggered when the screen is touched or clicked.
        /// </summary>
        public event UnityAction<Vector3> OnStickmanSelected;

        #endregion

        #region Unity Lifecycle Methods
        protected override void Awake()
        {
            base.Awake();   
        }
        private void Update()
        {
            HandleStickmanSelection();
        }
        #endregion

        #region Input Checking


        private void HandleStickmanSelection()
        {
            if (Input.GetMouseButtonDown(0) && Level.Instance != null) 
            {
                Vector3 mousePosition = Input.mousePosition;
                OnStickmanSelected?.Invoke(mousePosition);
            }

        }
        #endregion
    }
}
