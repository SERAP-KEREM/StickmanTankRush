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
    #endregion

    #region Constants
    private const float TankSpacing = 5f;
    private const int MaxStickmanCount = 3;
    #endregion

    #region Private Fields
    private LevelDataSO _levelDataSO;
    private readonly Queue<Tank> _tankQueue = new Queue<Tank>();
    private bool _isMoving;

    [SerializeField, ReadOnly]
    private Tank _currentTank;
    #endregion
    [SerializeField] private float _tankCheckDelay = 0.2f;
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
    public void SetLevelDataSO(LevelDataSO levelDataSO)
    {
        _levelDataSO = levelDataSO;
    }

    public void MoveNextTankToStopPoint()
    {
        if (_isMoving)
        {
            Debug.Log("[TankManager] Cannot move next tank - movement in progress");
            return;
        }

        if (_currentTank != null && _currentTank.IsFull)
        {
            Debug.Log("[TankManager] Starting movement sequence for full tank");
            StartCoroutine(MoveTankAndPrepareNext());
        }
        else if (_tankQueue.Count > 0 && _currentTank == null)
        {
            Debug.Log("[TankManager] Preparing first tank");
            PrepareNextTank();
        }
    }

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
        if (_levelDataSO == null)
        {
            Debug.LogError("[TankManager] LevelDataSO reference is missing!");
            return;
        }

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

        Debug.Log("[TankManager] Starting tank movement sequence");

        // Current tank'ın hareketini başlat
        if (_currentTank != null && _currentTank.IsFull)
        {
            AudioManager.Instance.PlayAudio(AudioKeys.TANK_MOVE);
            _currentTank.MoveToTank();
            _currentTank.CurrentState = TankState.Moving;

            // Current tank null yapılmadan önce yeni tankı hazırla
            _currentTank = null;

            // Çok kısa bir bekleme ile yeni tankı getir
            yield return new WaitForSeconds(0.5f);

            if (_tankQueue.Count > 0)
            {
                Debug.Log("[TankManager] Preparing next tank immediately");
                PrepareNextTank();
            }
            else
            {
                Debug.Log("[TankManager] No more tanks in queue");
                CheckAllTanksLeft();
            }
        }

        _isMoving = false;
    }
    private void PrepareNextTank()
    {
        if (_tankQueue.Count == 0)
        {
            Debug.Log("[TankManager] No tanks in queue to prepare");
            return;
        }

        if (Tank.IsAnyTankMoving)
        {
            Debug.Log("[TankManager] Cannot prepare next tank while another is moving");
            return;
        }

        _currentTank = _tankQueue.Dequeue();
        _currentTank.Initialize(_startPosition);
        _currentTank.CurrentState = TankState.Filling;

        Debug.Log($"[TankManager] New tank prepared: {_currentTank.name}");
        StartCoroutine(WaitForTankPositionAndCheckHolders());
    
    }

    private IEnumerator WaitForTankPositionAndCheckHolders()
    {
        if (_currentTank == null) yield break;

        Debug.Log("[TankManager] Waiting for tank to reach position");

        // Tank'ın hareket etmesini ve durmasını bekle
        while (_currentTank.IsMoving)
        {
            yield return null;
        }

        // Ek güvenlik beklemesi
        yield return new WaitForSeconds(0.5f);

        // Tank hazır, şimdi holder kontrolü yapılabilir
        if (_currentTank != null && !_currentTank.IsFull)
        {
            Debug.Log("[TankManager] Tank in position, checking holders");
            NotifyGameManagerForHolderCheck();
        }
    }
    private void NotifyGameManagerForHolderCheck()
    {
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
        {
            Debug.Log("[TankManager] All tanks have left!");
            OnAllTanksLeft?.Invoke();
        }
    }
    private bool ValidateReferences()
    {
        if (_tankPrefab == null)
        {
            Debug.LogError("[TankManager] Tank prefab is missing!");
            return false;
        }

        if (_levelDataSO?.TankDataList == null || _levelDataSO.TankDataList.Count == 0)
        {
            Debug.LogError("[TankManager] Level data contains no tank configurations!");
            return false;
        }

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
