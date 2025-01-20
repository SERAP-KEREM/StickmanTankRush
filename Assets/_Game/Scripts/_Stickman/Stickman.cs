using _Main;
using _Main._Enums;
using _Main._Tank;
using DG.Tweening;
using SerapKeremGameTools.Game._Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField, Range(90f, 720f)] private float _rotationSpeed = 720f;

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
        get=> _isMoving;
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

    /// <summary>
    /// Grid X position of the Stickman.
    /// </summary>
    public int GridX { get; private set; }

    /// <summary>
    /// Grid Y position of the Stickman.
    /// </summary>
    public int GridY { get; private set; }

    #endregion
    [SerializeField] private float _arrivalThreshold = 0.1f;
    #region NavMesh Components
    private NavMeshAgent _agent;
    private NavMeshObstacle _obstacle;
    private bool _isNavMeshInitialized;
    #endregion
    [Header("State")]
    [SerializeField] private bool _isInHolder;

    public bool IsInHolder
    {
        get => _isInHolder;
         set => _isInHolder = value;
    }
    #region Unity Methods

    private void Awake()
    {
        _tileGrid = FindObjectOfType<TileGrid>();
        _gridPathFinder = FindObjectOfType<GridPathFinder>();
        _tankManager = FindObjectOfType<TankManager>();
        if (_tileGrid == null)
            Debug.LogError("[Stickman] TileGrid not found!");
        if (_gridPathFinder == null)
            Debug.LogError("[Stickman] GridPathFinder not found!");

        // Set the collider to trigger mode
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.isTrigger = true;
        }
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_obstacle == null) _obstacle = GetComponent<NavMeshObstacle>();
        // Assign layer for this Stickman
        SetLayer("MovingStickman");
        SetupNavMeshComponents();

    }
    private void SetupNavMeshComponents()
    {
        // Agent setup
        _agent = gameObject.AddComponent<NavMeshAgent>();
        _agent.radius = 0.3f;
        _agent.height = 1f;
        _agent.speed = _moveSpeed;
        _agent.angularSpeed = _rotationSpeed;
        _agent.acceleration = 8f;
        _agent.stoppingDistance = 0.1f;
        _agent.enabled = false;

        // Obstacle setup
        _obstacle = gameObject.AddComponent<NavMeshObstacle>();
        _obstacle.carving = true;
        _obstacle.radius = 0.3f;
        _obstacle.height = 1f;
        _obstacle.enabled = true;
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
        StartCoroutine(DelayedNavMeshSetup());

    }
    private IEnumerator DelayedNavMeshSetup()
    {
        yield return new WaitForEndOfFrame();

        try
        {
            Debug.Log($"[Stickman] Setting up NavMesh for {gameObject.name}");

            if (_agent == null)
            {
                _agent = gameObject.AddComponent<NavMeshAgent>();
                _agent.radius = 0.3f;
                _agent.height = 1f;
                _agent.speed = _moveSpeed;
                _agent.angularSpeed = _rotationSpeed;
                _agent.acceleration = 8f;
                _agent.stoppingDistance = 0.1f;
                _agent.enabled = false;
            }

            if (_obstacle == null)
            {
                _obstacle = gameObject.AddComponent<NavMeshObstacle>();
                _obstacle.carving = true;
                _obstacle.radius = 0.3f;
                _obstacle.height = 1f;
                _obstacle.enabled = true;
            }

            _isNavMeshInitialized = true;
            Debug.Log($"[Stickman] NavMesh setup complete for {gameObject.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Stickman] Error in NavMesh setup: {e}");
        }
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
        Debug.Log($"[Stickman] Position set to [{x},{y}]");
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

        // DURUM 1: Holder'daysa direkt tanka git
        if (IsInHolder)
        {
            var currentTank = _tankManager?.CurrentTank;
            if (currentTank != null && currentTank.UnitColorType == UnitColorType)
            {
                Debug.Log("[Stickman] Direct movement from holder to tank");
                IsInHolder = false;
                DirectMoveToTank(currentTank);
                return;
            }
        }

        // DURUM 2: Y=0'daysa (en ön s?ra) direkt tanka git
        if (GridY == 0)
        {
            Debug.Log("[Stickman] Direct movement from front row to tank");
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            currentTile?.RemoveStickman();
            DirectMoveToTank(_tankManager.CurrentTank);
            return;
        }

        // DURUM 3: Di?er tile'lardaki stickmanlar için path finding
        if (_gridPathFinder.HasValidPathToTarget(this))
        {
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            currentTile?.RemoveStickman();

            _currentPath = _gridPathFinder.GetPathPoints();
            if (_currentPath != null && _currentPath.Count > 0)
            {
                _currentPath.Add(targetPosition);
                StartPathMovement(tankTransform);
            }
        }
    }

    private void DirectMoveToTank(Tank tank)
    {
        if (tank == null) return;

        _isMoving = true;
        Vector3 targetPos = tank.GetStickmanTargetPosition();

        // Önce parent'? ayarla
        transform.SetParent(tank.transform);

        // Direkt hareket et
        transform.DOMove(targetPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                _isMoving = false;
                tank.OnStickmanArrived(this);
            });
    }
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

        foreach (Vector3 nextPosition in _currentPath)
        {
            currentPoint++;

            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("[Stickman] Stickman was destroyed during movement!");
                yield break;
            }

            Debug.Log($"[Stickman] Moving to point {currentPoint}/{totalPoints}");

            // Önce dön
            yield return StartCoroutine(RotateTowards(nextPosition));

            if (!gameObject.activeInHierarchy) yield break;

            // Sonra hareket et
            yield return StartCoroutine(MoveToPosition(nextPosition));

            if (!gameObject.activeInHierarchy) yield break;

            Debug.Log($"[Stickman] Reached point {currentPoint}/{totalPoints}");
        }

        if (gameObject.activeInHierarchy)
        {
            CompleteMovement(target);
        }
    }

    private IEnumerator RotateTowards(Vector3 targetPosition)
    {
        if (!gameObject.activeInHierarchy) yield break;

        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            if (!gameObject.activeInHierarchy) yield break;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if (!gameObject.activeInHierarchy) yield break;

        while (Vector3.Distance(transform.position, targetPosition) > _arrivalThreshold)
        {
            if (!gameObject.activeInHierarchy) yield break;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                _moveSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
    private void CompleteMovement(Transform target)
    {
        _isMoving = false;
        IsSelectable = false;

        if (target != null)
        {
            if (target.TryGetComponent<Tank>(out var tank))
            {
                Debug.Log($"[Stickman] Completing movement to tank {tank.name}");
                transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                // Tank'a fiziksel olarak ba?la
                transform.SetParent(tank.transform);

                // Holder durumunu güncelle
                IsInHolder = false;

                // Tank'a bildir
                tank.OnStickmanArrived(this);
            }
            else if (target.TryGetComponent<Holder>(out var holder))
            {
                Debug.Log($"[Stickman] Completing movement to holder {holder.name}");
                IsInHolder = true;
                holder.AssignStickman(this);
            }
        }
        else
        {
            Debug.LogWarning("[Stickman] Movement completed but target is null!");
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

        // En öndeyse direkt holder'a git
        if (GridY == 0)
        {
            Debug.Log($"[Stickman] Direct movement to holder from [{GridX},{GridY}]");
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            currentTile?.RemoveStickman();
            DirectMove(holderPosition, null);
            return;
        }

        // Path bul ve hareket et
        if (_gridPathFinder.HasValidPathToTarget(this))
        {
            Debug.Log($"[Stickman] Found path to holder from [{GridX},{GridY}]");
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            currentTile?.RemoveStickman();

            _currentPath = _gridPathFinder.GetPathPoints();
            if (_currentPath != null && _currentPath.Count > 0)
            {
                _currentPath.Add(holderPosition); // Son hedef
                StartPathMovement(null);
            }
        }
    }
    private void StartPathMovement(Transform target)
    {
        if (_currentPath == null || _currentPath.Count == 0)
        {
            Debug.LogError("[Stickman] Cannot start path movement with null or empty path!");
            return;
        }

        _isMoving = true;
        Debug.Log($"[Stickman] Starting path movement with {_currentPath.Count} points");
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
        //currentTile?.RemoveStickman();

        //_isMoving = true;
        //targetPosition.y = 0f;

        StartCoroutine(DirectMoveSequence(targetPosition, newParent));
    }
    private IEnumerator DirectMoveSequence(Vector3 targetPosition, Transform target)
    {
 
        // Hedefe dön
        yield return StartCoroutine(RotateTowards(targetPosition));

        if (!gameObject.activeInHierarchy) yield break;

        // Hedefe git
        yield return StartCoroutine(MoveToPosition(targetPosition));

        if (!gameObject.activeInHierarchy) yield break;

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

        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

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
        Renderer childRenderer = transform.GetChild(0).GetComponent<Renderer>();
        if (childRenderer != null)
        {
            childRenderer.material.color = ColorManager.ColorTypeToColor(_colorType);
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
