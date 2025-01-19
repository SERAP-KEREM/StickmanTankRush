using System.Collections;
using UnityEngine;
using _Main._Enums;
using DG.Tweening;
using System.Collections.Generic;
using TriInspector; // DOTween included

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

    /// <summary>
    /// Represents a tank that can collect and transport stickmen.
    /// Handles tank movement, stickman collection, and state management.
    /// </summary>
    public class Tank : MonoBehaviour
    {
        #region Events
        public event System.Action<TankState> OnStateChanged;
        #endregion

        #region Configuration
        [Title("Tank Settings")]
        [SerializeField, PropertyTooltip("Maximum number of stickmen the tank can hold")]
        [Range(1, 5)]
        private int _maxStickmanCount = 3;

        [SerializeField, PropertyTooltip("Duration of tank movement in seconds")]
        [Range(1f, 10f)]
        private float _movementDuration = 1f;

        [SerializeField, PropertyTooltip("The color type of this tank")]
        private ColorType _colorType;

        [Title("Stickman Positioning")]
        [SerializeField, PropertyTooltip("Distance in front of tank where stickmen stop")]
        [Range(1f, 5f)]
        private float _stickmanStopDistance = 2f;

        [SerializeField, PropertyTooltip("Height at which stickmen should stay")]
        private float _stickmanHeight = 0f;

        [Title("Debug")]
        [SerializeField, PropertyTooltip("Show target position in scene view")]
        private bool _showTargetPosition = true;

        [SerializeField, PropertyTooltip("Color for target position visualization")]
        private Color _targetPositionColor = Color.green;

        [Title("Stickman Positioning")]
        [SerializeField] private float _stickmanSpacing = 0.5f;
        [SerializeField] private Vector3 _firstStickmanOffset = new Vector3(0.5f, 0f, 0f);
        [SerializeField] private float _nextTankDelay = 0.2f;
        /// <summary>
        /// Gets the maximum number of stickmen this tank can hold.
        /// </summary>
        public int MaxStickmanCount => _maxStickmanCount;
   

        private readonly Vector3[] _stickmanPositions = new Vector3[]
 {
       new Vector3(1f, -0.5f, 0f),   
       new Vector3(0f, -0.5f, 0f),    
       new Vector3(-1f, -0.5f, 0f)    
 };
        #endregion

        #region State
        private TankState _currentState = TankState.Waiting;
        private bool _isMoving;
        private bool _isReadyForStickmen;
        private Vector3 _targetPosition;
        private int _stickmanCount;
        private int _arrivedStickmanCount;
        private readonly List<Stickman> _attachedStickmen = new List<Stickman>();

        public static bool IsAnyTankMoving { get; private set; }

        /// <summary>
        /// Gets whether the tank has reached its maximum stickman capacity.
        /// </summary>
        public bool IsFull => _stickmanCount >= _maxStickmanCount;

        /// <summary>
        /// Gets the current number of stickmen in the tank.
        /// </summary>
        public int StickmanCount => _stickmanCount;

        /// <summary>
        /// Gets or sets the color type of the tank.
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
        /// Gets or sets the current state of the tank.
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the tank with a target position and prepares it for operation.
        /// </summary>
        public void Initialize(Vector3 target)
        {
            _targetPosition = target;
            CurrentState = TankState.Waiting;
            _isReadyForStickmen = false;
            UpdateColor();

            MoveToInitialPosition(target);
        }

        /// <summary>
        /// Adds a stickman to the tank if color matches and tank isn't full.
        /// </summary>
        public void AddStickman(ColorType stickmanColor)
        {
            if (!CanAddStickman(stickmanColor)) return;

            _stickmanCount++;
            CheckForFullTank();
        }

        /// <summary>
        /// Checks if the tank is full and updates its state accordingly.
        /// </summary>
        private void CheckForFullTank()
        {
            if (_stickmanCount >= _maxStickmanCount)
            {
                SetTankStateToMoving();
            }
        }
        /// <summary>
        /// Sets the tank's state to moving and initiates movement.
        /// </summary>
        private void SetTankStateToMoving()
        {
            CurrentState = TankState.Moving;
            MoveToTank();
        }
        /// <summary>
        /// Initiates tank movement to its target position.
        /// </summary>
        public void MoveToTank()
        {
            if (_currentState != TankState.Moving || !_isMoving) return;

            const float distanceFactor = 25f;
            _targetPosition = transform.position + Vector3.left * distanceFactor;

            transform.DOMove(_targetPosition, _movementDuration)
                .SetEase(Ease.Linear)
                .OnComplete(OnMovementComplete);
        }

        /// <summary>
        /// Called when a stickman reaches the tank.
        /// </summary>
        public void OnStickmanArrived(Stickman stickman)
        {
            if (!_isReadyForStickmen) return;

            if (_attachedStickmen.Contains(stickman)) return;

            _attachedStickmen.Add(stickman);
            _arrivedStickmanCount++;

            int positionIndex = _attachedStickmen.Count - 1;
            UpdateStickmanPosition(stickman, positionIndex);

            if (ShouldStartMovement())
            {
                StartCoroutine(StartMovement());
            }
        }

        /// <summary>
        /// Gets the target position for the next stickman.
        /// </summary>
        public Vector3 GetStickmanTargetPosition()
        {
            Vector3 tankPosition = transform.position;

            int nextIndex = _attachedStickmen.Count;
            if (nextIndex >= _stickmanPositions.Length) return tankPosition;

            Vector3 targetPosition = transform.TransformPoint(_stickmanPositions[nextIndex]);

            return targetPosition;
        }
        #endregion

        #region Private Methods
        private void MoveToInitialPosition(Vector3 target)
        {
            transform.DOMove(target, 1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => _isReadyForStickmen = true);
        }

        private bool CanAddStickman(ColorType stickmanColor)
        {
            return stickmanColor == _colorType && !IsFull;
        }

        private bool ShouldStartMovement()
        {
            return _arrivedStickmanCount >= _maxStickmanCount && IsFull;
        }

        private void UpdateColor()
        {
            var newColor = ColorManager.ColorTypeToColor(_colorType);
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = newColor;
            }
        }

        private void UpdateStickmanPosition(Stickman stickman, int index)
        {
            if (index >= _stickmanPositions.Length) return;

            stickman.transform.SetParent(transform);

            stickman.transform.DOLocalMove(_stickmanPositions[index], 0.5f)
           .SetEase(Ease.OutQuad);

            stickman.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), 0.5f)
                .SetEase(Ease.OutQuad);


        }
        private IEnumerator StartMovement()
        {
            if (IsAnyTankMoving)
            {
                Debug.Log("[Tank] Waiting for other tank to complete movement");
                yield break;
            }

            yield return new WaitForSeconds(0.2f);

            InitiateMovement();
        }

        private void InitiateMovement()
        {
            _isMoving = true;
            IsAnyTankMoving = true;
            CurrentState = TankState.Moving;

            _targetPosition = transform.position + Vector3.left * 10f;

            transform.DOMove(_targetPosition, _movementDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    OnMovementComplete();
                    StartCoroutine(PrepareForNextTank());
                });
        }
        private IEnumerator PrepareForNextTank()
        {
            yield return new WaitForSeconds(_nextTankDelay);
            IsAnyTankMoving = false;
        }

        private void OnMovementComplete()
        {
            _isMoving = false;
        }

        private void OnDrawGizmos()
        {
            if (!_showTargetPosition) return;

            Gizmos.color = _targetPositionColor;
            Vector3 targetPos = GetStickmanTargetPosition();
            Gizmos.DrawWireSphere(targetPos, 0.3f);
            Gizmos.DrawLine(transform.position, targetPos);
        }
        #endregion
    }
}
