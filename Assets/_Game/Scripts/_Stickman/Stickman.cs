using _Main._Enums;
using UnityEngine;
using DG.Tweening;
using SerapKeremGameTools.Game._Interfaces;
using UnityEngine.AI;
using System.Collections;

namespace _Main._Stickman.StickmanGrid
{
    /// <summary>
    /// Represents a Stickman character that can move and interact within the grid.
    /// </summary>
    public class Stickman : MonoBehaviour, ISelectable
    {
    
        #region Fields & Properties

        [Header("Stickman Configuration")]
        [SerializeField, Tooltip("Determines the color type of the Stickman.")]
        private ColorType _colorType;

        [SerializeField, Tooltip("Indicates if the Stickman is selectable.")]
        private bool _isSelectable = true;

        [SerializeField, Range(0.1f, 5f), Tooltip("Move speed of the Stickman.")]
        private float _moveSpeed = 1f;

        private StickmanGrid _stickmanGrid;

        private NavMeshAgent _navMeshAgent;
        [Header("Movement")]
        
        [SerializeField] private float _stoppingDistance = 0.1f;

        [Header("Path Finding")]
        [SerializeField] private float _pathUpdateInterval = 0.1f;
        [SerializeField] private float _pathEndThreshold = 0.1f;
        [SerializeField] private float _rotationSpeed = 720f;
        private bool _isMoving;
        private Vector3 _currentTargetPosition;
        private Transform _targetParent;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the color type of the Stickman.
        /// </summary>
        public ColorType UnitColorType
        {
            get => _colorType;
            set => _colorType = value;
        }

        /// <summary>
        /// Gets or sets whether the Stickman is selectable.
        /// </summary>
        public bool IsSelectable
        {
            get => _isSelectable;
            set => _isSelectable = value;
        }

        /// <summary>
        /// Gets the current grid X position of the Stickman.
        /// </summary>
        public int GridX { get; private set; }

        /// <summary>
        /// Gets the current grid Y position of the Stickman.
        /// </summary>
        public int GridY { get; private set; }

        #endregion
        #region ISelectable Implementation
        public void Select()
        {
            if (!IsSelectable) return;

            _isSelectable = true;
            GameManager.Instance.HandleStickmanSelection(this);
            Debug.Log($"[Stickman] Selected: {name}");
        }

        public void DeSelect()
        {
            _isSelectable = false;
            Debug.Log($"[Stickman] Deselected: {name}");
        }
        #endregion
        #region Unity Methods

        private void Awake()
        {
            // Cache StickmanGrid reference for performance optimization
            _stickmanGrid = FindObjectOfType<StickmanGrid>();
            if (_stickmanGrid == null)
            {
                Debug.LogError("No StickmanGrid instance found in the scene.");
            }
           
        }
        private void Start()
        {
            SetupNavMeshAgent();
        }
        private void SetupNavMeshAgent()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            if (_navMeshAgent == null)
                _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();

            _navMeshAgent.speed = _moveSpeed;
            _navMeshAgent.stoppingDistance = _stoppingDistance;
            _navMeshAgent.acceleration = 8f;
            _navMeshAgent.angularSpeed = _rotationSpeed;
            _navMeshAgent.updateRotation = false;
        }
            private void Update()
        {
            if (_isMoving && _navMeshAgent.enabled)
            {
                // Hedefe ula??ld? m? kontrol et
                if (Vector3.Distance(transform.position, _currentTargetPosition) <= _pathEndThreshold)
                {
                    OnReachedDestination();
                }
            }
        }
        private void OnReachedDestination()
        {
            if (_isMoving && _navMeshAgent.velocity.magnitude > 0.1f)
            {
                // Hareket yönüne do?ru yumu?ak dönü?
                Vector3 direction = _navMeshAgent.velocity.normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        _rotationSpeed * Time.deltaTime
                    );
                }
            }

            if (_targetParent != null)
            {
                transform.SetParent(_targetParent);
                IsSelectable = false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the grid position of the Stickman.
        /// </summary>
        /// <param name="x">X position in the grid.</param>
        /// <param name="y">Y position in the grid.</param>
        public void SetGridPosition(int x, int y)
        {
            GridX = x;
            GridY = y;
        }

        /// <summary>
        /// Initializes the Stickman by setting its color based on its assigned color type.
        /// </summary>
        public void Initialize()
        {
            IsSelectable = true;
            Renderer childRenderer = transform.GetChild(0).GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.material.color = ColorManager.ColorTypeToColor(_colorType);
            }
            else
            {
                Debug.LogError("Renderer component not found on Stickman.");
            }
        }

        /// <summary>
        /// Moves the Stickman to the specified target position using DOTween.
        /// </summary>
        /// <param name="targetPosition">The target position where the Stickman will move.</param>
        /// <param name="tankTransform">The tank's transform to attach the Stickman to after movement.</param>
        public void MoveToTank(Vector3 targetPosition, Transform tankTransform)
        {
            if (_stickmanGrid != null)
            {
                targetPosition.z = -targetPosition.z; // Ensure the z-axis is flipped (if necessary for your setup)
                MoveToPosition(targetPosition, tankTransform);
            }
        }

        /// <summary>
        /// Moves the Stickman to a specified holder position.
        /// </summary>
        /// <param name="holderPosition">The target holder position.</param>
        public void MoveToHolder(Vector3 holderPosition)
        {
            Vector3 targetPosition = holderPosition;
            targetPosition.y = transform.position.y; // Maintain current Y position
            MoveToPosition(targetPosition);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// General method for moving the Stickman to a specified position.
        /// </summary>
        /// <param name="targetPosition">The target position where the Stickman will move.</param>
        /// <param name="tankTransform">Optional: The tank's transform to attach the Stickman to after movement.</param>
        private void MoveToPosition(Vector3 targetPosition, Transform tankTransform = null)
        {
            //transform.DOMove(targetPosition, _moveSpeed)
            //    .SetEase(Ease.Linear)
            //    .OnComplete(() =>
            //    {
            //        if (tankTransform != null)
            //        {
            //            transform.SetParent(tankTransform);
            //            IsSelectable = false;
            //        }
            //    });
            if (_navMeshAgent == null) return;

            _isMoving = true;
            _navMeshAgent.SetDestination(targetPosition);
            StartCoroutine(WaitForDestination(targetPosition, tankTransform));
        }
        private IEnumerator WaitForDestination(Vector3 targetPosition, Transform tankTransform)
        {
            while (_navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial ||
                   Vector3.Distance(transform.position, targetPosition) > _stoppingDistance)
            {
                yield return null;
            }

            _isMoving = false;

            if (tankTransform != null)
            {
                transform.SetParent(tankTransform);
                IsSelectable = false;
            }
        }
        #endregion
    }
}
