using _Main._Stickman.StickmanGrid;
using SerapKeremGameTools._Game._InputSystem;
using SerapKeremGameTools._Game._Singleton;
using UnityEngine;

namespace _Main._StickmanSelector
{
    /// <summary>
    /// Handles the selection of Stickman units by the player.
    /// </summary>
    public class StickmanSelector : MonoSingleton<StickmanSelector>
    {
        [Header("Raycast Settings")]
        [SerializeField, Tooltip("Maximum distance for raycast detection")]
        private float _raycastLength = 10f;

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            if (transform.parent == null)
            {
                transform.SetParent(FindObjectOfType<Level>()?.transform);
            }
        }

        private void OnEnable()
        {
            if (PlayerInput.Instance != null)
            {
                PlayerInput.Instance.OnMouseDownEvent.AddListener(HandleSelection);
            }
        }

        private void OnDisable()
        {
            if (PlayerInput.Instance != null)
            {
                PlayerInput.Instance.OnMouseDownEvent.RemoveListener(HandleSelection);
            }
        }
        #endregion

        #region Selection Handling
        private void HandleSelection()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera is not assigned in the scene.");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _raycastLength))
            {
                if (hit.collider.TryGetComponent<Stickman>(out var stickman) && stickman.IsSelectable)
                {
                    GameManager.Instance.HandleStickmanSelection(stickman);
                    Debug.Log($"[GameSelector] Selected stickman: {stickman.name}");
                }
            }
        }
        #endregion
    }
}
