using _Main._Enums;
using _Main._Tank;
using DG.Tweening;
using LevelEditor;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the initialization, configuration, and behavior of tanks in the game.
/// </summary>
public class TankManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    [SerializeField, Tooltip("ScriptableObject containing tank data and configurations.")]
    private LevelDataSO _levelDataSO;

    [SerializeField, Tooltip("Prefab reference for the tank.")]
    private Tank _tankPrefab;

    [SerializeField, Tooltip("List of stop points where tanks will move to.")]
    private List<Transform> stopPoints;

    private Queue<Tank> tankQueue = new Queue<Tank>();
    [SerializeField] private Tank currentTank;

    private const float TankSpacing = 10f;

    [SerializeField, Tooltip("Initial spawn position for tanks.")]
    private Vector3 startPosition = Vector3.zero;

    private const int MaxStickmanCount = 3;

    /// <summary>
    /// Sets up the tanks and moves the first one to the stop point.
    /// </summary>
    private void Start()
    {
        SetupTanks();
        MoveNextTankToStopPoint();
    }

    /// <summary>
    /// Initializes tanks based on the data from LevelDataSO.
    /// </summary>
    private void SetupTanks()
    {
        if (_levelDataSO == null || _tankPrefab == null || stopPoints.Count == 0)
        {
            Debug.LogError("Setup failed: Missing required references.");
            return;
        }

        foreach (var tankData in _levelDataSO.TankDataList)
        {
            Vector3 position = startPosition + Vector3.right * TankSpacing * tankQueue.Count;
            Tank newTank = Instantiate(_tankPrefab, position, Quaternion.identity, transform);
            newTank.UnitColorType = tankData.TankColorType;
            newTank.Initialize(stopPoints[0].position);

            newTank.name = $"{tankData.TankColorType} Tank [{tankQueue.Count}]";
            tankQueue.Enqueue(newTank);
        }
    }

    /// <summary>
    /// Handles user input and performs tank-related updates.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintCurrentTankInfo();
        }
    }

    /// <summary>
    /// Moves the next tank in the queue to the stop point.
    /// </summary>
    public void MoveNextTankToStopPoint()
    {
        if (tankQueue.Count == 0)
        {
            Debug.LogWarning("No tanks left in the queue.");
            return;
        }

        if (currentTank != null && currentTank.StickmanCount >= MaxStickmanCount)
        {
            currentTank.MoveToTarget();
            currentTank.CurrentState = TankState.Moving;
        }
      

        currentTank = tankQueue.Dequeue();
        currentTank.Initialize(stopPoints[0].position);
        currentTank.CurrentState = TankState.Filling;

        Debug.Log($"Next tank {currentTank.name} is now at the stop point.");
    }
    /// <summary>
    /// Kuyru?un geri kalan?ndaki tanklar? 10 birim ilerletir.
    /// </summary>


public void MoveOtherTanks()
{
    // Tüm tanklar? s?ras?yla hareket ettiriyoruz
    foreach (var tank in tankQueue)
    {
        // Tank?n mevcut pozisyonunu al?yoruz
        Vector3 currentPosition = tank.transform.position;

        // Yeni hedef pozisyonu belirliyoruz
        Vector3 targetPosition = new Vector3(currentPosition.x - TankSpacing, currentPosition.y, currentPosition.z);

        // DOTween ile tank? hareket ettiriyoruz
        tank.transform.DOMove(targetPosition, 3f) // 1 saniyede hareket etmesi için
            .SetEase(Ease.Linear); // Hareketin e?it h?zda olmas?n? sa?l?yoruz
    }

    // Current tank için ayn? i?lemi uyguluyoruz
    Vector3 currentTankPosition = currentTank.transform.position;
    Vector3 targetCurrentTankPosition = new Vector3(currentTankPosition.x - TankSpacing, currentTankPosition.y, currentTankPosition.z);

    // DOTween ile current tank? hareket ettiriyoruz
    currentTank.transform.DOMove(targetCurrentTankPosition, 3f)
        .SetEase(Ease.Linear);
}

/// <summary>
/// Logs information about the currently active tank.
/// </summary>
private void PrintCurrentTankInfo()
    {
        if (currentTank == null)
        {
            Debug.Log("No active tank at the moment.");
            return;
        }

        Debug.Log($"Active Tank: {currentTank.name}, Color: {currentTank.UnitColorType}, Stickmen: {currentTank.StickmanCount}");
    }

    /// <summary>
    /// Returns the currently controlled tank.
    /// </summary>
    /// <returns>The currently active tank.</returns>
    public Tank GetCurrentTank()
    {
        return currentTank;
    }
}
