using UnityEngine;
using _Main._Enums;
using _Main._Tank;
using DG.Tweening;
using LevelEditor;
using SerapKeremGameTools._Game._AudioSystem;
using System.Collections.Generic;
using System.Collections;
using TriInspector;
using _Main;

/// <summary>
/// Manages tank creation, movement, and state transitions.
/// </summary>
[DeclareFoldoutGroup("Configuration", Title = "Tank Settings")]
public class TankManager : MonoBehaviour
{
    #region Configuration
    [Group("Configuration")]
    [SerializeField, Required]
    [PropertyTooltip("Prefab reference for the tank")]
    private Tank _tankPrefab;

    [Group("Configuration")]
    [SerializeField]
    [PropertyTooltip("Duration of tank movement")]
    [Range(1f, 10f)]
    private float _moveDuration = 0.5f;

    [Group("Configuration")]
    [SerializeField]
    [PropertyTooltip("Initial spawn position")]
    private Vector3 _startPosition = Vector3.zero;

    [Group("Configuration")]
    [SerializeField, Tooltip("Delay between tank checks")]
    private float _tankCheckDelay = 0.2f;
    #endregion

    #region Constants
    private const float TankSpacing = 4f;
    private const int MaxStickmanCount = 3;
    #endregion

    #region Private Fields
    private LevelDataSO _levelDataSO;
    private readonly Queue<Tank> _tankQueue = new Queue<Tank>();
    private bool _isMoving;

    [SerializeField, ReadOnly]
    private Tank _currentTank;
    #endregion

    #region Properties
    public Tank CurrentTank => _currentTank;
    #endregion

    #region Events
    public event System.Action OnAllTanksLeft;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeTanks();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the level data for the tank manager.
    /// </summary>
    /// <param name="levelDataSO">The LevelDataSO to set.</param>
    public void SetLevelDataSO(LevelDataSO levelDataSO)
    {
        _levelDataSO = levelDataSO;
    }

    /// <summary>
    /// Moves the next tank to its stop point.
    /// </summary>
    public void MoveNextTankToStopPoint()
    {
        if (_isMoving) return;

        if (_currentTank != null && _currentTank.IsFull)
            StartCoroutine(MoveTankAndPrepareNext());

        else if (_tankQueue.Count > 0 && _currentTank == null)
            PrepareNextTank();
    }

    /// <summary>
    /// Moves the other tanks in the queue.
    /// </summary>
    public void MoveOtherTanks()
    {
        if (_isMoving) return;

        MoveQueueTanks();
        MoveCurrentTank();
    }
    #endregion

    #region Private Methods
    private void InitializeTanks()
    {
        if (_levelDataSO == null) return;

        SetupTanks();
        MoveNextTankToStopPoint();
    }

    private void SetupTanks()
    {
        ClearExistingTanks();

        if (!ValidateReferences()) return;

        CreateTanks();
    }

    private IEnumerator MoveTankAndPrepareNext()
    {
        if (_isMoving || Tank.IsAnyTankMoving) yield break;

        _isMoving = true;


        if (_currentTank != null && _currentTank.IsFull)
        {
            AudioManager.Instance.PlayAudio(AudioKeys.TANK_MOVE);
            _currentTank.MoveToTank();
            _currentTank.CurrentState = TankState.Moving;
            _currentTank = null;

            yield return new WaitForSeconds(0.5f);

            if (_tankQueue.Count > 0)
                PrepareNextTank();
            else
                CheckAllTanksLeft();
        }

        _isMoving = false;
    }
    private void PrepareNextTank()
    {
        if (_tankQueue.Count == 0) return;

        if (Tank.IsAnyTankMoving) return;

        _currentTank = _tankQueue.Dequeue();
        _currentTank.Initialize(_startPosition);
        _currentTank.CurrentState = TankState.Filling;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.MoveAllHolderStickmenToCurrentTank();
        }

    }
    private void MoveQueueTanks()
    {
        foreach (var tank in _tankQueue)
        {
            MoveTankToPosition(tank);
        }
    }

    private void MoveCurrentTank()
    {
        if (_currentTank != null)
        {
            MoveTankToPosition(_currentTank);
        }
    }
    private void MoveTankToPosition(Tank tank)
    {
        if (tank == null || _isMoving) return;

        Vector3 currentPosition = tank.transform.position;
        Vector3 targetPosition = new Vector3(
            currentPosition.x - TankSpacing,
            currentPosition.y,
            currentPosition.z
        );

        if (currentPosition.x > targetPosition.x)
        {
            tank.transform.DOMove(targetPosition, _moveDuration)
                .SetEase(Ease.Linear);
        }
    }
    private void CheckAllTanksLeft()
    {
        if (_tankQueue.Count == 0 && _currentTank == null)
            OnAllTanksLeft?.Invoke();
    }
    private bool ValidateReferences()
    {
        if (_tankPrefab == null) return false;

        if (_levelDataSO?.TankDataList == null || _levelDataSO.TankDataList.Count == 0) return false;

        return true;
    }

    private void ClearExistingTanks()
    {
        while (_tankQueue.Count > 0)
        {
            var tank = _tankQueue.Dequeue();
            if (tank != null) Destroy(tank.gameObject);
        }
        _currentTank = null;
    }

    private void CreateTanks()
    {
        foreach (var tankData in _levelDataSO.TankDataList)
        {
            CreateTank(tankData);
        }
    }
    private void CreateTank(TankData tankData)
    {
        Vector3 position = _startPosition + Vector3.right * TankSpacing * _tankQueue.Count;
        Tank newTank = Instantiate(_tankPrefab, position, Quaternion.identity);
        newTank.transform.SetParent(transform, worldPositionStays: false);

        ConfigureTank(newTank, tankData, position);
    }

    private void ConfigureTank(Tank tank, TankData data, Vector3 position)
    {
        tank.UnitColorType = data.TankColorType;
        tank.Initialize(position);
        tank.name = $"{data.TankColorType} Tank [{_tankQueue.Count}]";
        _tankQueue.Enqueue(tank);
    }
    #endregion
}