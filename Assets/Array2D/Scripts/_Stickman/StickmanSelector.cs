using _Input;
using _Main._Stickman.StickmanGrid;
using UnityEngine;

namespace _Main._StickmanSelector
{
    /// <summary>
    /// Handles the selection of Stickman units by the player.
    /// </summary>
    public class StickmanSelector : MonoBehaviour
    {
        #region Unity Lifecycle Methods

        /// <summary>
        /// Subscribes to the OnStickmanSelected event when the object is enabled.
        /// </summary>
        private void OnEnable()
        {
            InputHandler.Instance.OnStickmanSelected += HandleStickmanSelection;
        }

        /// <summary>
        /// Unsubscribes from the OnStickmanSelected event when the object is disabled.
        /// </summary>
        private void OnDisable()
        {
            InputHandler.Instance.OnStickmanSelected -= HandleStickmanSelection;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the input event to determine if a Stickman was selected.
        /// </summary>
        /// <param name="screenPosition">The screen position of the touch or click.</param>
        private void HandleStickmanSelection(Vector3 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition); // Ray from screen position
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Stickman clickedStickman = hit.collider.GetComponent<Stickman>();
                if (clickedStickman != null && clickedStickman.IsSelectable)
                {
                    GameManager.Instance.HandleStickmanSelection(clickedStickman); // Pass the selected Stickman to the GameManager
                }
            }
        }


        #endregion
    }
}
