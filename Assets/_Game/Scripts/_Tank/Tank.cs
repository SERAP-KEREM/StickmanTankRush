using System.Collections;
using UnityEngine;
using _Main._Enums;
using DG.Tweening;
using System.Collections.Generic;
using TriInspector; 

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
        [Range(0.2f, 2f)]
        private float _movementDuration = 0.3f;

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
        [SerializeField] private float _nextTankDelay = 0.1f;

        private bool _isWaitingForStickmen = false;
        private bool _readyToMove = false;

        private Animator _animator;
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

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
      
        private int _arrivedStickmanCount;
        private readonly List<Stickman> _attachedStickmen = new List<Stickman>();

        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                _isMoving = value;
                if (_animator != null)
                {
                    _animator.SetBool(IsMovingHash, value);
                }
            }
        }
        public static bool IsAnyTankMoving { get; private set; }

        [Title("Stickman Settings")]
        [SerializeField]
        private float _stickmanPositioningDuration = 0.3f;

        /// <summary>
        /// Gets whether the tank has reached its maximum stickman capacity.
        /// </summary>
        public bool IsFull => _arrivedStickmanCount >= _maxStickmanCount;

        /// <summary>
        /// Gets the current number of stickmen in the tank.
        /// </summary>
        public int StickmanCount => _arrivedStickmanCount;

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
        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
            {
                Debug.LogWarning("[Tank] Animator component not found!");
            }
        }
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
        }

        /// <summary>
        /// Adds a stickman to the tank if color matches and tank isn't full.
        /// </summary>
        public void AddStickman(ColorType stickmanColor)
        {
            if (!CanAddStickman(stickmanColor)) return;

            _arrivedStickmanCount++;
            Debug.Log($"[Tank] Added stickman. Count: {_arrivedStickmanCount}/{_maxStickmanCount}");

            if (IsFull && !_isMoving && !_isWaitingForStickmen)
            {
                Debug.Log("[Tank] Tank is full, preparing for movement");
                StartCoroutine(PrepareForMovement());
            }
        }

        /// <summary>
        /// Sets the tank's state to moving and initiates movement.
        /// </summary>
        private void SetTankStateToMoving()
        {
            if (_isMoving) return;

            Debug.Log("[Tank] Setting state to Moving");
            CurrentState = TankState.Moving;
            _isMoving = true;
            IsAnyTankMoving = true;
            MoveToTank();
        }
        /// <summary>
        /// Initiates tank movement to its target position.
        /// </summary>
        public void MoveToTank()
        {
            if (!_isMoving)
            {
                Debug.LogWarning("[Tank] Trying to move while not in moving state");
                return;
            }

            Debug.Log("[Tank] Starting movement to target");
            const float distanceFactor = 40f; 
            _targetPosition = transform.position + Vector3.left * distanceFactor;

            IsMoving = true;

            transform.DOMove(_targetPosition, _movementDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    IsMoving = false;
                    Debug.Log("[Tank] Movement completed");
                    OnMovementComplete();
                });
        }

        /// <summary>
        /// Called when a stickman reaches the tank.
        /// </summary>
        public void OnStickmanArrived(Stickman stickman)
        {
            if (stickman == null) return;
            if (!_isReadyForStickmen) return;

            if (_attachedStickmen.Contains(stickman)) return;

            _attachedStickmen.Add(stickman);

            int positionIndex = _attachedStickmen.Count - 1;
            UpdateStickmanPosition(stickman, positionIndex);

            if (_readyToMove && _attachedStickmen.Count == _maxStickmanCount)
            {
                StartCoroutine(PrepareForMovement());
            }
        }

        private IEnumerator PrepareForMovement()
        {
            if (_isMoving || _isWaitingForStickmen) yield break;

            _isWaitingForStickmen = true;
            Debug.Log("[Tank] Waiting for stickmen to settle...");

            yield return new WaitForSeconds(0.2f);

            if (IsFull && !_isMoving)
            {
                Debug.Log("[Tank] All conditions met, starting movement");
                _isWaitingForStickmen = false;
                SetTankStateToMoving();
            }
            else
            {
                Debug.LogWarning($"[Tank] Movement cancelled. IsFull: {IsFull}, IsMoving: {_isMoving}");
                _isWaitingForStickmen = false;
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

        private bool CanAddStickman(ColorType stickmanColor)
        {
            return stickmanColor == _colorType && !IsFull;
        }
        private void UpdateColor()
        {
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();

            foreach (var renderer in allRenderers)
            {
                Material[] materials = renderer.materials;

                foreach (var material in materials)
                {
                    material.color = ColorManager.ColorTypeToColor(_colorType);
                }
            }
        }
        private void UpdateStickmanPosition(Stickman stickman, int index)
        {
            if (index >= _stickmanPositions.Length) return;

            stickman.transform.SetParent(transform);

            stickman.transform.DOLocalMove(_stickmanPositions[index], _stickmanPositioningDuration)
           .SetEase(Ease.OutQuad);

            stickman.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), _stickmanPositioningDuration)
                .SetEase(Ease.OutQuad);
        }
        private void OnMovementComplete()
        {
            Debug.Log($"[Tank] {name} completed movement");
            _isMoving = false;
            IsAnyTankMoving = false;

            var tankManager = FindObjectOfType<TankManager>();
            if (tankManager != null)
            {
                tankManager.MoveNextTankToStopPoint();
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
        #endregion
    }
}
