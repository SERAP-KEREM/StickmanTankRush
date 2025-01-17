using _Main;
using _Main._Enums;
using _Main._Tank;
using DG.Tweening;
using SerapKeremGameTools.Game._Interfaces;
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
    [SerializeField, Range(1f, 10f)] private float _moveSpeed = 2f;

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
    /// Grid X position of the Stickman.
    /// </summary>
    public int GridX { get; private set; }

    /// <summary>
    /// Grid Y position of the Stickman.
    /// </summary>
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

        // Set the collider to trigger mode
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.isTrigger = true;
        }

        // Assign layer for this Stickman
        SetLayer("MovingStickman");
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
        DirectMove(targetPosition, tankTransform);
    }

    /// <summary>
    /// Moves the Stickman directly to the holder's position.
    /// </summary>
    /// <param name="holderPosition">The holder position.</param>
    public void MoveToHolder(Vector3 holderPosition)
    {
        if (_isMoving) return;
        SetLayer("WaitingStickman");
        DirectMove(holderPosition);
    }

    /// <summary>
    /// Moves the Stickman along with the tank for a specified duration.
    /// </summary>
    /// <param name="targetPosition">The target position.</param>
    /// <param name="duration">The duration of the movement.</param>
    public void MoveWithTank(Vector3 targetPosition, float duration)
    {
        transform.DOMove(targetPosition, duration)
            .SetEase(Ease.Linear);
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
        currentTile?.RemoveStickman();

        _isMoving = true;
        targetPosition.y = 0f;

        transform.DOMove(targetPosition, _moveSpeed)
             .SetEase(Ease.Linear)
             .OnComplete(() =>
             {
                 _isMoving = false;
                 IsSelectable = false;

                 if (newParent != null && newParent.TryGetComponent<Tank>(out var tank))
                 {
                     transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                     tank.OnStickmanArrived(this);
                 }
             });
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
