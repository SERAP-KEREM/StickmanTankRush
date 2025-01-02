using _Main._Enums;
using _Main._Tank;
using DG.Tweening;
using LevelEditor;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    public static TankManager Instance { get; private set; }

    [Header("Tank Configuration")]
    [SerializeField, Tooltip("Prefab reference for the tank.")]
    private Tank _tankPrefab;

    [Header("Tank Movement Configuration")]
    [SerializeField, Tooltip("Duration of tank movement.")]
    [Range(1f, 10f)]
    private float _moveDuration = 3f;

    [SerializeField, Tooltip("Initial spawn position for tanks.")]
    private Vector3 _startPosition = Vector3.zero;

    private const float TankSpacing = 10f;
    private const int MaxStickmanCount = 3;

    private LevelDataSO _levelDataSO;

    private Queue<Tank> _tankQueue = new Queue<Tank>();

    [SerializeField, Tooltip("The current tank being filled or moved.")]
    private Tank _currentTank;

    public Tank CurrentTank
    {
        get => _currentTank;
        private set => _currentTank = value;
    }

    #region Singleton Pattern

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Make sure it persists between scenes
    }

    #endregion

    #region Level Setup

    /// <summary>
    /// Sets the level data containing tank configurations.
    /// </summary>
    public void SetLevelDataSO(LevelDataSO levelDataSO)
    {
        _levelDataSO = levelDataSO;
    }

    private void Start()
    {
        if (_levelDataSO == null)
        {
            Debug.LogError("LevelDataSO reference is missing in TankManager.");
            return;
        }

        SetupTanks();
        MoveNextTankToStopPoint();
    }

    /// <summary>
    /// Sets up the tanks based on level data.
    /// </summary>
    public void SetupTanks()
    {
        if (_tankPrefab == null)
        {
            Debug.LogError("Tank prefab is missing.");
            return;
        }
        Debug.Log("___________________________________________"+_levelDataSO.TankDataList.Count.ToString());
        if (_levelDataSO?.TankDataList == null || _levelDataSO.TankDataList.Count == 0)
        {
            Debug.LogError("Level data contains no tank configurations.");
            return;
        }

        // TankManager'ın pozisyonunu kontrol et
        if (transform.position == Vector3.zero)
        {
            Debug.LogWarning("TankManager position is at (0,0,0). Ensure it's set correctly.");
        }

        // Tankları yerleştirirken doğru pozisyon ayarları yapılacak
        for (int i = 0; i < _levelDataSO.TankDataList.Count; i++)
        {
            // Tankların sırasına göre pozisyon hesaplanır
            Vector3 position = _startPosition + Vector3.right * TankSpacing * i;
            TankData tankData = _levelDataSO.TankDataList[i];

            // Tank prefab'ını oluştur
            Tank newTank = Instantiate(_tankPrefab, position, Quaternion.identity, transform);
            newTank.UnitColorType = tankData.TankColorType; // Tankın rengi
            newTank.Initialize(position);

            newTank.name = $"{tankData.TankColorType} Tank [{i}]"; // Debug için isim
            _tankQueue.Enqueue(newTank);
        }
    }

    #endregion

    #region Tank Movement

    /// <summary>
    /// Moves the next tank to the stop point and handles the current tank's state.
    /// </summary>
    public void MoveNextTankToStopPoint()
    {
        if (_tankQueue.Count == 0)
        {
            Debug.LogWarning("No tanks left in the queue.");
            return;
        }

        // If current tank is full, move it first
        if (_currentTank != null && _currentTank.StickmanCount >= MaxStickmanCount)
        {
            _currentTank.MoveToTank();
            _currentTank.CurrentState = TankState.Moving;
        }

        // Get the next tank from the queue
        _currentTank = _tankQueue.Dequeue();
        _currentTank.Initialize(_startPosition);
        _currentTank.CurrentState = TankState.Filling;

        Debug.Log($"Next tank {_currentTank.name} is now active.");
    }

    /// <summary>
    /// Moves all tanks in the queue and the current tank.
    /// </summary>
    public void MoveOtherTanks()
    {
        MoveQueueTanks();
        MoveCurrentTank();
    }

    #endregion

    #region Tank Helpers

    /// <summary>
    /// Moves all tanks in the queue towards their target positions.
    /// </summary>
    private void MoveQueueTanks()
    {
        foreach (var tank in _tankQueue)
        {
            MoveTankToPosition(tank);
        }
    }

    /// <summary>
    /// Moves the current tank towards its target position.
    /// </summary>
    private void MoveCurrentTank()
    {
        if (_currentTank == null) return;

        MoveTankToPosition(_currentTank);
    }

    /// <summary>
    /// Helper method to move a tank to its target position.
    /// </summary>
    /// <param name="tank">The tank to be moved.</param>
    private void MoveTankToPosition(Tank tank)
    {
        Vector3 currentPosition = tank.transform.position;
        Vector3 targetPosition = new Vector3(currentPosition.x - TankSpacing, currentPosition.y, currentPosition.z);

        if (currentPosition.x > targetPosition.x)
        {
            tank.transform.DOMove(targetPosition, _moveDuration).SetEase(Ease.Linear);
        }
    }

    #endregion
}
