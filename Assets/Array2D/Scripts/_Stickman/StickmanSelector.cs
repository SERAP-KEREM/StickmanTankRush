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
        /// Handles the Stickman selection event.
        /// </summary>
        /// <param name="selectedStickman">The Stickman that has been selected.</param>
        private void HandleStickmanSelection(Stickman selectedStickman)
        {
            // Pass the selected Stickman to the GameManager for further processing
            GameManager.Instance.HandleStickmanSelection(selectedStickman);
        }

        #endregion
    }
}
