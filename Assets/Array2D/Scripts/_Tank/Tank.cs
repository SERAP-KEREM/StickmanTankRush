using System.Collections;
using UnityEngine;
using _Main._Enums;
using DG.Tweening; // DOTween included

namespace _Main._Tank
{
    /// <summary>
    /// Represents the states a tank can be in.
    /// </summary>
    public enum TankState
    {
        Waiting,  // Tank is waiting at the stop point.
        Filling,  // Tank is being filled with stickmen.
        Moving    // Tank is moving towards its target.
    }

    public class Tank : MonoBehaviour
    {
        public event System.Action<TankState> OnStateChanged;

        #region Tank Configuration

        [SerializeField, Tooltip("Maximum number of stickmen the tank can hold.")]
        private int maxStickmanCount = 3;

        [SerializeField, Tooltip("Current state of the tank.")]
        private TankState currentState = TankState.Waiting;

        public TankState CurrentState
        {
            get => currentState;
            set
            {
                if (currentState != value)
                {
                    currentState = value;
                    OnStateChanged?.Invoke(currentState);
                }
            }
        }

        [Tooltip("Current number of stickmen in the tank.")]
        private int stickmanCount;

        public int StickmanCount
        {
            get => stickmanCount;
            private set
            {
                if (stickmanCount != value)
                {
                    stickmanCount = value;
                    CheckForFullTank(); // Check if tank is full after each update
                }
            }
        }

        private Vector3 targetPosition;

        #endregion

        #region Tank Color Configuration

        [SerializeField, Tooltip("The color type of the tank.")]
        private ColorType _colorType;

        /// <summary>
        /// Gets or sets the color type of the tank. Updates the tank's color when set.
        /// </summary>
        public ColorType UnitColorType
        {
            get => _colorType;
            set
            {
                _colorType = value;
                UpdateColor();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the tank with a target position and updates its color.
        /// </summary>
        /// <param name="target">The target position for the tank.</param>
        public void Initialize(Vector3 target)
        {
            targetPosition = target;
            CurrentState = TankState.Waiting;
            UpdateColor();
        }

        /// <summary>
        /// Updates the tank's color based on its color type.
        /// </summary>
        private void UpdateColor()
        {
            Color newColor = ColorManager.ColorTypeToColor(_colorType);
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = newColor;
            }
        }

        /// <summary>
        /// Adds a stickman to the tank. If the tank is full, initiates movement to the target position.
        /// </summary>
        /// <param name="stickmanColor">The color type of the stickman being added.</param>
        public void AddStickman(ColorType stickmanColor)
        {
            if (stickmanColor != _colorType)
            {
                Debug.LogWarning($"Stickman color does not match tank color. Tank color: {_colorType}, Stickman color: {stickmanColor}");
                return;
            }

            StickmanCount++;  // Increment stickman count
            Debug.Log($"Stickman added. Current count: {stickmanCount}/{maxStickmanCount}");
        }

        /// <summary>
        /// Moves the tank to the target position based on a predefined distance factor. 
        /// Once the movement is complete, the tank is destroyed.
        /// </summary>
        public void MoveToTarget()
        {
            const float distanceFactor = 15f; // Distance the tank should move on the X axis
            targetPosition = new Vector3(transform.position.x - distanceFactor, transform.position.y, transform.position.z);

            Debug.Log($"Tank is moving from {transform.position} to {targetPosition}.");

            transform.DOMove(targetPosition, 5f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    Debug.Log("Tank has arrived at the target and will now be destroyed.");
                    Destroy(gameObject); // Destroy the tank once it reaches the target
                });
        }

        /// <summary>
        /// Determines whether the tank is full.
        /// </summary>
        /// <returns>True if the tank is full, false otherwise.</returns>
        public bool IsFull()
        {
            return stickmanCount >= maxStickmanCount;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if the tank is full, and if so, changes its state to moving.
        /// </summary>
        private void CheckForFullTank()
        {
            if (IsFull() && currentState != TankState.Moving)
            {
                SetTankStateToMoving();  // Transition to moving state only if full
            }
            else if (!IsFull() && currentState != TankState.Filling)
            {
                CurrentState = TankState.Filling;  // Tank is still being filled
            }
        }

        /// <summary>
        /// Sets the tank's state to moving and triggers the movement.
        /// </summary>
        private void SetTankStateToMoving()
        {
            if (CurrentState != TankState.Moving)
            {
                CurrentState = TankState.Moving;
                MoveToTarget();  // Start the movement once the tank is full
            }
        }

        #endregion
    }
}
