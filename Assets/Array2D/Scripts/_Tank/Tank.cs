using System.Collections;
using UnityEngine;
using _Main._Enums;

namespace _Main._Tank
{
    /// <summary>
    /// Enum representing the different states a tank can be in.
    /// </summary>
    public enum TankState
    {
        Waiting,  // Tank is waiting at the stop point.
        Filling,  // Tank is being filled with stickmen.
        Moving    // Tank is moving towards its target.
    }

    /// <summary>
    /// Tank class that controls the tank's state, movement, and stickman interactions.
    /// </summary>
    public class Tank : MonoBehaviour
    {
        [Header("Tank Configuration")]
        [Tooltip("Current state of the tank.")]
        public TankState currentState = TankState.Waiting;

        [Tooltip("Number of stickmen inside the tank.")]
        public int stickmanCount = 0;

        [Tooltip("Maximum number of stickmen the tank can hold.")]
        [SerializeField] private int maxStickmanCount = 3;

        [Tooltip("Target position where the tank is moving.")]
        private Vector3 targetPosition;

        [Header("Tank Color Configuration")]
        [SerializeField]
        [Tooltip("Color type of the tank.")]
        private ColorType _colorType;

        /// <summary>
        /// Property to get or set the tank's color type and automatically update its visual color.
        /// </summary>
        public ColorType UnitColorType
        {
            get => _colorType;
            set
            {
                _colorType = value;
                UpdateColor(); // Update color whenever the color type is changed.
            }
        }

        /// <summary>
        /// Initializes the tank with the given target position and sets its state to waiting.
        /// </summary>
        /// <param name="target">The target position where the tank will move.</param>
        public void Initialize(Vector3 target)
        {
            UpdateColor();  // Set the color based on the color type.
            targetPosition = target;  // Set the target position where the tank will move.
            currentState = TankState.Waiting;  // Set the tank's initial state to waiting.
        }

        /// <summary>
        /// Updates the visual color of the tank based on its color type.
        /// </summary>
        private void UpdateColor()
        {
            Color newColor = ColorManager.ColorTypeToColor(_colorType);  // Convert ColorType to Unity Color.
            Renderer[] renderers = GetComponentsInChildren<Renderer>();  // Get all child renderers (for tank model).
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = newColor;  // Apply the color to the renderer.
            }
        }

        /// <summary>
        /// Adds a stickman to the tank. If the tank is full, it starts moving.
        /// </summary>
        public void AddStickman(ColorType stickmanColor)
        {
            // Ensure the tank is in a state where stickmen can be added (waiting state).
            if (currentState != TankState.Waiting)
            {
                Debug.Log("Tank is not in waiting state. Cannot add stickman.");
                return;
            }

            // Ensure that the stickman's color matches the tank's color.
            if (stickmanColor != _colorType)
            {
                Debug.Log("Stickman color does not match the tank's color.");
                return;
            }

            // Increase the stickman count.
            stickmanCount++;
            Debug.Log($"Stickman added: {stickmanCount}");

            // Check if the tank is full (3 stickmen) and if so, start moving.
            if (IsFull())
            {
                currentState = TankState.Filling;  // Change tank state to filling.
                Debug.Log("Tank is full, moving...");
                StartMoving();  // Start the tank's movement.
            }
        }

        /// <summary>
        /// Starts moving the tank towards the target position.
        /// </summary>
        public void StartMoving()
        {
            // Only allow movement if the tank is in the filling state.
            if (currentState != TankState.Filling)
            {
                Debug.Log("Tank is not in filling state. Cannot start moving.");
                return;
            }

            currentState = TankState.Moving;  // Change the state to moving.
            StartCoroutine(MoveToTarget());  // Move the tank using a coroutine.
        }

        /// <summary>
        /// Moves the tank to its target position along the -X direction.
        /// </summary>
        private IEnumerator MoveToTarget()
        {
            // Ensure the target position is along the -X direction.
            targetPosition = new Vector3(transform.position.x - Mathf.Abs(targetPosition.x - transform.position.x),
                                         transform.position.y,
                                         transform.position.z);

            Debug.Log($"Moving from {transform.position} to {targetPosition}");

            // Move the tank towards the target position.
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    5f * Time.deltaTime
                );
                yield return null;  // Wait for the next frame.
            }

            Debug.Log("Arrived at the target!");
            Destroy(gameObject);  // Destroy the tank once it reaches its target.
        }

        /// <summary>
        /// Checks if the tank is full.
        /// </summary>
        /// <returns>True if the tank is full, otherwise false.</returns>
        public bool IsFull()
        {
            return stickmanCount >= maxStickmanCount;
        }
    }
}
