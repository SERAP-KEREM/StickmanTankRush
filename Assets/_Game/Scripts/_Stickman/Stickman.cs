using _Main;
using _Main._Enums;
using _Main._Tank;
using DG.Tweening;
using SerapKeremGameTools.Game._Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a Stickman unit with movement, selection, and color-based functionalities.
/// </summary>
public class Stickman : MonoBehaviour, ISelectable
{
    #region Fields

    [Header("Stickman Configuration")]
    [Tooltip("Color type of the Stickman, used for matching with Tanks or Holders.")]
    [SerializeField] private ColorType _colorType;

    [Tooltip("Indicates whether this Stickman can be selected.")]
    [SerializeField] private bool _isSelectable = true;

    [Tooltip("Movement speed of the Stickman.")]
    [SerializeField, Range(1f, 10f)] private float _moveSpeed = 5f;

    [Tooltip("Rotation speed of the Stickman in degrees per second.")]
    [SerializeField, Range(90f, 720f)] private float _rotationSpeed = 360f;

    [Header("Movement")]
    [Tooltip("Threshold distance to determine when the Stickman reaches a path point.")]
    [SerializeField, Range(0.01f, 0.5f)] private float _pathPointThreshold = 0.1f;

    private TileGrid _tileGrid;
    private GridPathFinder _gridPathFinder;
    private bool _isMoving;
    private List<Vector3> _currentPath;
    private int _currentPathIndex;
    private Transform _targetParent;
    private TankManager _tankManager;

    private Animator _animator;
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");

    [SerializeField] private float _arrivalThreshold = 0.1f;

    [Header("State")]
    [SerializeField] private bool _isInHolder;
    public bool IsInHolder
    {
        get => _isInHolder;
        set => _isInHolder = value;
    }
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
    public bool IsMoving
    {
        get => _isMoving;
        set => _isMoving = value;
    }

    /// <summary>
    /// Gets or sets whether the Stickman is selectable.
    /// </summary>
    public bool IsSelectable
    {
        get => _isSelectable;
        set => _isSelectable = value;
    }

    public int GridX { get; private set; }

    public int GridY { get; private set; }

    #endregion

    #region Unity Methods
    private void Awake()
    {
        _tileGrid = FindObjectOfType<TileGrid>();
        _gridPathFinder = FindObjectOfType<GridPathFinder>();
        _tankManager = FindObjectOfType<TankManager>();
        _animator = GetComponentInChildren<Animator>();

        if (_tileGrid == null)
            Debug.LogError("[Stickman] TileGrid not found!");

        if (_gridPathFinder == null)
            Debug.LogError("[Stickman] GridPathFinder not found!");

        if (_animator == null)
            Debug.LogError("[Stickman] Animator not found!");

        // Set the collider to trigger mode
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.isTrigger = true;
        }
        SetLayer("MovingStickman");
    }
    private void StartMovementAnimation()
    {
        _animator?.SetBool(IsRunning, true);
    }

    private void StopMovementAnimation()
    {
        _animator?.SetBool(IsRunning, false);
    }

    private void Update()
    {
        if (_isMoving && _currentPath != null)
        {
            UpdateMovement();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the Stickman, setting it to selectable and updating its color.
    /// </summary>
    public void Initialize()
    {
        IsSelectable = true;
        UpdateColor();

    }
    /// <summary>
    /// Sets the grid position of the Stickman.
    /// </summary>
    /// <param name="x">Grid X position.</param>
    /// <param name="y">Grid Y position.</param>
    public void SetGridPosition(int x, int y)
    {
        GridX = x;
        GridY = y;
    }

    /// <summary>
    /// Selects the Stickman, triggering the GameManager's selection handling.
    /// </summary>
    public void Select()
    {
        if (!IsSelectable) return;
        GameManager.Instance.HandleStickmanSelection(this);
    }

    /// <summary>
    /// Deselects the Stickman, making it non-selectable.
    /// </summary>
    public void DeSelect()
    {
        IsSelectable = false;
    }

    /// <summary>
    /// Moves the Stickman directly to the tank's position.
    /// </summary>
    /// <param name="targetPosition">The target position.</param>
    /// <param name="tankTransform">The tank transform to attach to.</param>
    public void MoveToTank(Vector3 targetPosition, Transform tankTransform)
    {
        if (_isMoving) return;
        SetLayer("MovingStickman");
        StartMovementAnimation();

        var currentTank = tankTransform.GetComponent<Tank>();
        if (currentTank == null) return;

        if (IsInHolder)
        {
            IsInHolder = false;
            transform.SetParent(currentTank.transform);

            Vector3 tankPos = currentTank.GetStickmanTargetPosition();
            transform.DOMove(tankPos, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    StopMovementAnimation();
                    transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    _isMoving = false;
                    currentTank.OnStickmanArrived(this);
                });
            return;
        }
        if (GridY == 0)
        {
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            currentTile.RemoveStickman();
            DirectMoveToTank(currentTank);
            return;
        }

        if (_gridPathFinder.HasValidPathToTarget(this))
        {
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            currentTile.RemoveStickman();
            _currentPath = _gridPathFinder.GetPathPoints();

            if (_currentPath.Count > 0)
            {
                _currentPath.Add(targetPosition);
                StartPathMovement(tankTransform);
            }
        }
    }
    #endregion
    #region Helper Methods
    /// <summary>
    /// Moves the Stickman directly to the tank's position, with animation.
    /// </summary>
    private void DirectMoveToTank(Tank tank)
    {
        if (tank == null) return;

        _isMoving = true;
        Vector3 targetPos = tank.GetStickmanTargetPosition();

        // Attach the Stickman to the Tank
        transform.SetParent(tank.transform);

        // Move Stickman to the target position with animation
        transform.DOMove(targetPos, 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Finalize position and stop animation
                transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                _isMoving = false;
                StopMovementAnimation();
                tank.OnStickmanArrived(this);
            });
    }

    /// <summary>
    /// Follows a path to a target, handling movement and rotation step by step.
    /// </summary>
    private IEnumerator FollowPath(Transform target)
    {
        if (_currentPath == null || _currentPath.Count == 0)
        {
            Debug.LogError("[Stickman] Path is null or empty!");
            CompleteMovement(target);
            yield break;
        }

        int totalPoints = _currentPath.Count;
        int currentPoint = 0;

        StartMovementAnimation();

        foreach (Vector3 nextPosition in _currentPath)
        {
            currentPoint++;
            if (!CheckActive()) yield break;

            yield return StartCoroutine(RotateTowards(nextPosition));

            yield return StartCoroutine(MoveToPosition(nextPosition));


            if (!CheckActive()) yield break;
            Debug.Log($"[Stickman] Reached point {currentPoint}/{totalPoints}");
        }

        StopMovementAnimation();

        if (gameObject.activeInHierarchy)
        {
            CompleteMovement(target);
        }
    }
    /// <summary>
    /// Rotates the Stickman towards a target position.
    /// </summary>
    private IEnumerator RotateTowards(Vector3 targetPosition)
    {
        if (!CheckActive()) yield break;

     

        if (IsInHolder)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            yield break;
        }

        if (transform.parent?.GetComponent<Tank>() != null)
        {
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            yield break;
        }

        Vector3 moveDirection = targetPosition - transform.position;
        moveDirection.y = 0;

        if (moveDirection.magnitude < 0.1f) yield break;

        float xDiff = Mathf.Abs(moveDirection.x);
        float zDiff = Mathf.Abs(moveDirection.z);

        if (xDiff > zDiff && xDiff > 0.1f)
        {
            float targetAngle = moveDirection.x > 0 ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Checks if the Stickman is active in the hierarchy. Used to prevent movement if destroyed.
    /// </summary>
    private bool CheckActive()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[Stickman] Stickman was destroyed during movement!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Moves the Stickman towards the specified position, considering the fixed Y coordinate.
    /// </summary>
    /// <param name="targetPosition">Target position to move to.</param>
    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if (!gameObject.activeInHierarchy) yield break;

        float fixedY = transform.position.y;
        Vector3 targetWithFixedY = new Vector3(targetPosition.x, fixedY, targetPosition.z);

        while (Vector3.Distance(transform.position, targetWithFixedY) > _arrivalThreshold)
        {
            if (!gameObject.activeInHierarchy) yield break;


            Vector3 newPosition = Vector3.MoveTowards(
                transform.position,
                targetWithFixedY,
                _moveSpeed * Time.deltaTime
            );

            transform.position = new Vector3(newPosition.x, fixedY, newPosition.z);
            yield return null;
        }
        transform.position = targetWithFixedY;
    }


    /// <summary>
    /// Completes the Stickman's movement, either to a tank or a holder.
    /// </summary>
    /// <param name="target">The target where the Stickman moves (tank or holder).</param>
    private void CompleteMovement(Transform target)
    {
        _isMoving = false;
        IsSelectable = false;

        if (target != null)
        {
            if (target.TryGetComponent<Tank>(out var tank))
            {
                transform.SetParent(tank.transform);
                IsInHolder = false;
                tank.OnStickmanArrived(this);
            }
            else if (target.TryGetComponent<Holder>(out var holder))
            {
                IsInHolder = true;
                holder.AssignStickman(this);
            }
        }
    }

    /// <summary>
    /// Moves the Stickman directly to the holder's position.
    /// </summary>
    /// <param name="holderPosition">The holder position.</param>
    public void MoveToHolder(Vector3 holderPosition)
    {
        if (_isMoving) return;
        SetLayer("WaitingStickman");

        // If the Stickman is already at the correct grid position, directly move to the holder.
        if (GridY == 0)
        {
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            currentTile?.RemoveStickman();
            DirectMove(holderPosition, null);
            return;
        }

        // Use pathfinding if necessary.
        if (_gridPathFinder.HasValidPathToTarget(this))
        {
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            currentTile?.RemoveStickman();

            _currentPath = _gridPathFinder.GetPathPoints();
            if (_currentPath != null && _currentPath.Count > 0)
            {
                _currentPath.Add(holderPosition);// Add the holder's position to the path.
                StartPathMovement(null); // Start movement along the path.
            }
        }
    }

    /// <summary>
    /// Starts the movement along a calculated path.
    /// </summary>
    /// <param name="target">Optional target to finish the movement at.</param>
    private void StartPathMovement(Transform target)
    {
        if (_currentPath == null || _currentPath.Count == 0) return;
   
        _isMoving = true;

        // Start the coroutine to move along the path.
        StartCoroutine(FollowPath(target));
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Moves the Stickman directly to a target position.
    /// </summary>
    /// <param name="targetPosition">The target position.</param>
    /// <param name="newParent">The new parent transform, if any.</param>
    private void DirectMove(Vector3 targetPosition, Transform newParent = null)
    {
        var currentTile = _tileGrid.GetTileAt(GridX, GridY);
        _isMoving = true;
        StartCoroutine(DirectMoveSequence(targetPosition, newParent));
    }

    /// <summary>
    /// Sequence for moving the Stickman directly to a target position.
    /// </summary>
    /// <param name="targetPosition">The target position.</param>
    /// <param name="target">The new parent transform, if any.</param>
    private IEnumerator DirectMoveSequence(Vector3 targetPosition, Transform target)
    {
        StartMovementAnimation();
        yield return StartCoroutine(RotateTowards(targetPosition));

        if (!gameObject.activeInHierarchy) yield break;

        yield return StartCoroutine(MoveToPosition(targetPosition));

        if (!gameObject.activeInHierarchy) yield break;

        StopMovementAnimation();
        CompleteMovement(target);
    }
    /// <summary>
    /// Updates the Stickman's movement along the current path.
    /// </summary>
    private void UpdateMovement()
    {
        if (_currentPathIndex >= _currentPath.Count)
        {
            OnReachedDestination();
            return;
        }

        Vector3 targetPos = _currentPath[_currentPathIndex];
        targetPos.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            _moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < _pathPointThreshold)
        {
            _currentPathIndex++;
        }
    }

    /// <summary>
    /// Called when the Stickman reaches its final destination.
    /// </summary>
    private void OnReachedDestination()
    {
        _isMoving = false;
        _currentPath = null;
        _currentPathIndex = 0;

        if (_targetParent != null)
        {
            transform.SetParent(_targetParent);
            IsSelectable = false;
        }
    }

    /// <summary>
    /// Updates the color of the Stickman based on its color type.
    /// </summary>
    private void UpdateColor()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in allRenderers)
        {
            renderer.material.color = ColorManager.ColorTypeToColor(_colorType);
        }
    }

    /// <summary>
    /// Sets the layer for the Stickman and its children.
    /// </summary>
    /// <param name="layerName">The layer name.</param>
    private void SetLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);

        foreach (Transform child in transform)
        {
            child.gameObject.layer = gameObject.layer;
        }
    }
    #endregion
}
