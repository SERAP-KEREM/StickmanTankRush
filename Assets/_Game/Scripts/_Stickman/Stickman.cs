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
        // Assign layer for this Stickman
        SetLayer("MovingStickman");
    }
    private void StartMovementAnimation()
    {
        if (_animator != null)
        {
            _animator.SetBool(IsRunning, true);
        }
    }
    private void StopMovementAnimation()
    {
        if (_animator != null)
        {
            _animator.SetBool(IsRunning, false);
        }
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

        StartMovementAnimation();
        var currentTank = tankTransform.GetComponent<Tank>();
        if (currentTank == null) return;

        // DURUM 1: Holder'daysa direkt tank'a git
        if (IsInHolder)
        {
            Debug.Log("[Stickman] Direct movement from holder to tank");
            IsInHolder = false;

            // Direkt tank'a parent'la
            transform.SetParent(currentTank.transform);

            // Güncel pozisyona git
            Vector3 tankPos = currentTank.GetStickmanTargetPosition();
            transform.DOMove(tankPos, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    StopMovementAnimation();
                    transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    _isMoving = false;
                    currentTank.OnStickmanArrived(this);
                });

            return;
        }
        // DURUM 2: Y=0'daysa (en ön s?ra)
        if (GridY == 0)
        {
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            if (currentTile != null)
            {
                currentTile.RemoveStickman();
                DirectMoveToTank(currentTank);
            }
            return;
        }

        // DURUM 3: Di?er tile'lardaki stickmanlar için path finding
        if (_gridPathFinder.HasValidPathToTarget(this))
        {
            var currentTile = _tileGrid.GetTileAt(GridX, GridY);
            if (currentTile != null)
            {
                currentTile.RemoveStickman();
                _currentPath = _gridPathFinder.GetPathPoints();
                if (_currentPath != null && _currentPath.Count > 0)
                {
                    _currentPath.Add(targetPosition);
                    StartPathMovement(tankTransform);
                }
            }
        }
    }

    private void DirectMoveToTank(Tank tank)
    {
        if (tank == null) return;

        _isMoving = true;
        Vector3 targetPos = tank.GetStickmanTargetPosition();

        transform.SetParent(tank.transform);
        transform.DOMove(targetPos, 0.5f)
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

        StartMovementAnimation();

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

        StopMovementAnimation();

        if (gameObject.activeInHierarchy)
        {
            CompleteMovement(target);
        }
    }

    private IEnumerator RotateTowards(Vector3 targetPosition)
    {
        if (!gameObject.activeInHierarchy) yield break;

        // Holder kontrolü
        if (IsInHolder)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            yield break;
        }

        // Tank kontrolü
        if (transform.parent?.GetComponent<Tank>() != null)
        {
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            yield break;
        }

        // Hareket yönünü hesapla
        Vector3 moveDirection = targetPosition - transform.position;
        moveDirection.y = 0;

        // Çok küçük hareket varsa dönme
        if (moveDirection.magnitude < 0.1f) yield break;

        // Grid bazl? hareket yönünü belirle
        float xDiff = Mathf.Abs(moveDirection.x);
        float zDiff = Mathf.Abs(moveDirection.z);

        // Sadece X ekseni hareketi varsa (sa?a veya sola) rotasyon uygula
        if (xDiff > zDiff && xDiff > 0.1f)
        {
            float targetAngle = moveDirection.x > 0 ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        else
        {
            // Z ekseni hareketi veya kar???k hareket varsa düz bak
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if (!gameObject.activeInHierarchy) yield break;

        // Y pozisyonunu ba?lang?çta sabitle
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

            // Y pozisyonunu koru
            transform.position = new Vector3(newPosition.x, fixedY, newPosition.z);
            yield return null;
        }

        // Son pozisyonda da Y'yi koru
        transform.position = targetWithFixedY;
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

        _isMoving = true;
    

        StartCoroutine(DirectMoveSequence(targetPosition, newParent));
    }
    private IEnumerator DirectMoveSequence(Vector3 targetPosition, Transform target)
    {
        StartMovementAnimation();
        // Hedefe dön
        yield return StartCoroutine(RotateTowards(targetPosition));

        if (!gameObject.activeInHierarchy) yield break;

        // Hedefe git
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

      // Pozisyonu güncelle
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
        // Stickman modelinin tüm alt ö?elerini kontrol ediyoruz
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in allRenderers)
        {
            // Her renderer'? kontrol edip materyali de?i?tirebiliriz
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
