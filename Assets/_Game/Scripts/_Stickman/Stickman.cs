using _Main;
using _Main._Enums;
using _Main._Tank;
using DG.Tweening;
using SerapKeremGameTools.Game._Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class Stickman : MonoBehaviour, ISelectable
{
    #region Fields
    [Header("Stickman Configuration")]
    [SerializeField] private ColorType _colorType;
    [SerializeField] private bool _isSelectable = true;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 360f;

    [Header("Movement")]
    [SerializeField] private float _pathPointThreshold = 0.1f;

    private TileGrid _tileGrid;
    private GridPathFinder _gridPathFinder;
    private bool _isMoving;
    private List<Vector3> _currentPath;
    private int _currentPathIndex;
    private Transform _targetParent;
    #endregion

    #region Properties
    public ColorType UnitColorType
    {
        get => _colorType;
        set => _colorType = value;
    }
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

        if (_tileGrid == null)
            Debug.LogError("[Stickman] TileGrid not found!");
        if (_gridPathFinder == null)
            Debug.LogError("[Stickman] GridPathFinder not found!");
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
    public void Initialize()
    {
        IsSelectable = true;
        UpdateColor();
    }

    public void SetGridPosition(int x, int y)
    {
        GridX = x;
        GridY = y;
        Debug.Log($"[Stickman] Position set to [{x},{y}]");
    }

    public void Select()
    {
        if (!IsSelectable) return;
        GameManager.Instance.HandleStickmanSelection(this);
    }

    public void DeSelect()
    {
        IsSelectable = false;
    }

    public void MoveToTank(Vector3 targetPosition, Transform tankTransform)
    {
        if (_isMoving)
        {
            Debug.LogWarning("[Stickman] Already moving!");
            return;
        }

        // z=0 kontrolü
        if (GridY == 0)
        {
            DirectMove(targetPosition, tankTransform);
            return;
        }

        if (_gridPathFinder.HasValidPathToTarget(this))
        {
            _currentPath = _gridPathFinder.GetPathPositions();
            if (_currentPath != null && _currentPath.Count > 0)
            {
                StartMovement(targetPosition, tankTransform);
            }
        }
        else
        {
            Debug.LogWarning("[Stickman] No valid path to tank!");
        }
    }
    public void MoveToHolder(Vector3 holderPosition)
    {
        if (_isMoving)
        {
            Debug.LogWarning("[Stickman] Already moving!");
            return;
        }

        // z=0 kontrolü
        if (GridY == 0)
        {
            DirectMove(holderPosition);
            return;
        }

        if (_gridPathFinder.HasValidPathToTarget(this))
        {
            _currentPath = _gridPathFinder.GetPathPositions();
            if (_currentPath != null && _currentPath.Count > 0)
            {
                StartMovement(holderPosition);
            }
        }
        else
        {
            Debug.LogWarning("[Stickman] No valid path to holder!");
        }
    }
    #endregion

    #region Private Methods
    private void DirectMove(Vector3 targetPosition, Transform newParent = null)
    {
        // Mevcut tile'dan ayr?l
        var currentTile = _tileGrid.GetTileAt(GridX, GridY);
        if (currentTile != null)
        {
            currentTile.RemoveStickman();
            Debug.Log($"[Stickman] Removed from tile [{GridX},{GridY}]");
        }

        _isMoving = true;
        transform.DOMove(targetPosition, _moveSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _isMoving = false;
                if (newParent != null)
                {
                    transform.SetParent(newParent);
                    IsSelectable = false;
                    Debug.Log("[Stickman] Reached destination and attached to parent");
                }
            });
    }

    private void StartMovement(Vector3 targetPosition, Transform newParent = null)
    {
        // Mevcut tile'dan ayr?l
        var currentTile = _tileGrid.GetTileAt(GridX, GridY);
        if (currentTile != null)
        {
            currentTile.RemoveStickman();
            Debug.Log($"[Stickman] Removed from tile [{GridX},{GridY}]");
        }

        _currentPath = _gridPathFinder.GetPathPositions();
        if (_currentPath == null || _currentPath.Count == 0)
        {
            Debug.LogError("[Stickman] Path positions not found!");
            return;
        }

        _isMoving = true;
        _currentPathIndex = 0;
        _targetParent = newParent;

        Debug.Log($"[Stickman] Starting movement with path length: {_currentPath.Count}");
    }

    private void UpdateMovement()
    {
        if (_currentPathIndex >= _currentPath.Count)
        {
            OnReachedDestination();
            return;
        }

        Vector3 targetPos = _currentPath[_currentPathIndex];
        targetPos.y = transform.position.y; // Y pozisyonunu koru

        // Hedefe do?ru dön
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

        // Hedefe do?ru hareket et
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            _moveSpeed * Time.deltaTime
        );

        // Bir sonraki noktaya geç
        if (Vector3.Distance(transform.position, targetPos) < _pathPointThreshold)
        {
            _currentPathIndex++;
            Debug.Log($"[Stickman] Moving to next path point: {_currentPathIndex}/{_currentPath.Count}");
        }
    }

    private void OnReachedDestination()
    {
        _isMoving = false;
        _currentPath = null;
        _currentPathIndex = 0;

        if (_targetParent != null)
        {
            transform.SetParent(_targetParent);
            IsSelectable = false;
            Debug.Log("[Stickman] Reached final destination and attached to parent");
        }
    }

    private void UpdateColor()
    {
        Renderer childRenderer = transform.GetChild(0).GetComponent<Renderer>();
        if (childRenderer != null)
        {
            childRenderer.material.color = ColorManager.ColorTypeToColor(_colorType);
        }
    }
    #endregion
}