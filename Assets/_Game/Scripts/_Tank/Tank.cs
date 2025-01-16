using System.Collections;
using UnityEngine;
using _Main._Enums;
using DG.Tweening;
using System.Collections.Generic; // DOTween included

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

        [Header("Tank Configuration")]
        [SerializeField, Tooltip("Maximum number of stickmen the tank can hold.")]
        public int MaxStickmanCount = 3;

        [SerializeField, Tooltip("Current state of the tank.")]
        private TankState _currentState = TankState.Waiting;

        [SerializeField, Tooltip("Movement duration of the tank.")]
        private float _movementDuration = 5f;
        public float MovementDuration => _movementDuration;
        [Header("Stickman Settings")]
        [SerializeField]
        [Tooltip("Distance in front of the tank where stickmen should stop")]
        private float _stickmanStopDistance = 2f;

        [SerializeField]
        [Tooltip("Height at which stickmen should stay")]
        private float _stickmanHeight = 0f;

        [Header("Debug Visualization")]
        [SerializeField] private bool _showTargetPosition = true;
        [SerializeField] private Color _targetPositionColor = Color.green;
        private bool _isMovingToTarget = false;

        private int _arrivedStickmanCount = 0;
        private bool _isReadyToMove = false;
        private List<(Stickman stickman, Vector3 offset)> _attachedStickmen = new List<(Stickman, Vector3)>();
        private bool _isMoving = false;
        /// <summary>
        /// Gets whether the tank is full based on the current stickman count.
        /// </summary>
        public bool IsFull => _stickmanCount >= MaxStickmanCount;

        /// <summary>
        /// Gets or sets the current state of the tank. Triggers an event when state changes.
        /// </summary>
        public TankState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    OnStateChanged?.Invoke(_currentState);
                }
            }
        }
        private void OnDrawGizmos()
        {
            if (!_showTargetPosition) return;

            Gizmos.color = _targetPositionColor;
            Vector3 targetPos = GetStickmanTargetPosition();
            Gizmos.DrawWireSphere(targetPos, 0.3f);
            Gizmos.DrawLine(transform.position, targetPos);
        }
        [Tooltip("Current number of stickmen in the tank.")]
        private int _stickmanCount;

        public int StickmanCount
        {
            get => _stickmanCount;
            private set
            {
                if (_stickmanCount != value)
                {
                    _stickmanCount = value;
                    CheckForFullTank(); // Check if tank is full after each update
                }
            }
        }

        private Vector3 _targetPosition;

        #endregion

        #region Tank Color Configuration

        [Header("Tank Color Configuration")]
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
            _targetPosition = target;
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
            if (stickmanColor != _colorType || _isMovingToTarget) return;

            StickmanCount++;
        }

        public void OnStickmanArrived()
        {
            _arrivedStickmanCount++;
            Debug.Log($"[Tank] Stickman arrived. Arrived: {_arrivedStickmanCount}, Total: {StickmanCount}");

            if (_arrivedStickmanCount >= MaxStickmanCount && IsFull)
            {
                StartCoroutine(StartMovementSequence());
            }
        }
        private IEnumerator StartMovementSequence()
        {
            // Tüm stickman'lerin yerle?mesi için k?sa bir bekleme
            yield return new WaitForSeconds(0.5f);

            _isMoving = true;
            CurrentState = TankState.Moving;
            MoveToTank();
        }
        /// <summary>
        /// Moves the tank to the target position based on a predefined distance factor. 
        /// Once the movement is complete, the tank is destroyed.
        /// </summary>
        public void MoveToTank()
        {
            if (!_isMoving) return;

            const float distanceFactor = 25f;
            _targetPosition = new Vector3(transform.position.x - distanceFactor, transform.position.y, transform.position.z);

            // Önce tank hareket etsin
            transform.DOMove(_targetPosition, _movementDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    Debug.Log($"[Tank] Movement completed");
                    _isMoving = false;
                });

            // Stickman'leri de hareket ettir
            foreach (var (stickman, offset) in _attachedStickmen)
            {
                if (stickman != null)
                {
                    stickman.FollowTank(this, offset);
                }
            }
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if the tank is full, and if so, changes its state to moving.
        /// </summary>
        private void CheckForFullTank()
        {
            //Debug.Log($"Tank {gameObject.name}: Checking full status. Count: {_stickmanCount}/{_maxStickmanCount}, Current State: {_currentState}");

            if (_stickmanCount >= MaxStickmanCount)
            {
               // Debug.Log($"Tank {gameObject.name} should move now!");
                SetTankStateToMoving();
            }
        }

        /// <summary>
        /// Sets the tank's state to moving and triggers the movement.
        /// </summary>
        private void SetTankStateToMoving()
        {
            if (!_isReadyToMove) return;
            //Debug.Log($"Setting tank {gameObject.name} state to Moving.");
            CurrentState = TankState.Moving;
            MoveToTank();
        }
        public Vector3 GetStickmanTargetPosition()
        {
            float xOffset = (_stickmanCount - 1) * -1f;
            Vector3 offset = new Vector3(xOffset, 0f, -3f);

            // Offset'i kaydet
            if (_attachedStickmen.Count < MaxStickmanCount)
            {
                _attachedStickmen.Add((null, offset));
            }

            return transform.position + offset;
        }
        #endregion
    }
}