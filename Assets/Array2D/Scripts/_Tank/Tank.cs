using System.Collections;
using _Main._Enums;
using UnityEngine;

/// <summary>
/// The Tank class handles the state, color, movement, and stickman addition for a tank.
/// </summary>
namespace _Main._Tank
{
    /// <summary>
    /// Enum representing the different states a tank can be in.
    /// </summary>
    public enum TankState
    {
        Waiting,  // Waiting at the stop point
        Filling,  // Stickman is filling the tank
        Moving    // The tank is moving forward
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
                UpdateColor();
            }
        }

        /// <summary>
        /// Initializes the tank with the given target position and sets its state to waiting.
        /// </summary>
        /// <param name="target">The target position where the tank will move.</param>
        public void Initialize(Vector3 target)
        {
            UpdateColor();  // Update the tank's color
            targetPosition = target;  // Set the target position
            currentState = TankState.Waiting;  // Set the initial state to waiting
        }

        /// <summary>
        /// Updates the visual color of the tank based on its color type.
        /// </summary>
        private void UpdateColor()
        {
            Color newColor = ColorManager.ColorTypeToColor(_colorType);  // Convert the color type to Unity color
            Renderer[] renderers = GetComponentsInChildren<Renderer>();  // Get all renderers of the tank's children
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = newColor;  // Apply the color to each renderer
            }
        }

        /// <summary>
        /// Adds a stickman to the tank. If the tank is full, it starts moving.
        /// </summary>
        public void AddStickman()
        {
            if (currentState != TankState.Waiting) return;  // If the tank is not in the waiting state, do nothing

            stickmanCount++;  // Increase the stickman count
            Debug.Log($"Stickman added: {stickmanCount}");

            // If the tank is full, start the moving process
            if (stickmanCount >= 3)
            {
                currentState = TankState.Filling;
                Debug.Log("Tank is full, moving...");
                StartMoving();
            }
        }

        /// <summary>
        /// Starts moving the tank towards the target position.
        /// </summary>
        public void StartMoving()
        {
            if (currentState != TankState.Filling) return;  // If the tank is not full, do nothing

            currentState = TankState.Moving;
            StartCoroutine(MoveToTarget());  // Start the movement towards the target
        }

        /// <summary>
        /// Moves the tank towards the target position.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveToTarget()
        {
            // Move towards the target position until it's close enough
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5f * Time.deltaTime);
                yield return null;
            }

            Debug.Log("Arrived at the target!");
            Destroy(gameObject);  // Destroy the tank when it reaches the target
        }

        /// <summary>
        /// Draws a Gizmo in the scene view to visualize the tank's target position.
        /// </summary>
        private void OnDrawGizmos()
        {
            // If the target position is set, draw a line and sphere to visualize it in the editor
            if (targetPosition != Vector3.zero)
            {
                Gizmos.color = Color.green;  // Use green color for the target visualization
                Gizmos.DrawLine(transform.position, targetPosition);  // Draw a line to the target
                Gizmos.DrawSphere(targetPosition, 0.5f);  // Draw a sphere at the target position
            }
        }
    }
}
