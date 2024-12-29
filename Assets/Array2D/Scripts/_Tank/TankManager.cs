using _Main._Enums;
using _Main._Tank;
using DG.Tweening;
using LevelEditor;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    public static TankManager Instance { get; private set; }

    [Header("Grid Configuration")]
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

    private LevelDataSO _levelDataSO; // LevelDataSO alan?

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // E?er ba?ka bir instance varsa, yenisini yok et
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Sahneler aras?nda kal?c? hale getir
    }

    // LevelDataSO'yu almak için bir metod
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

    private void SetupTanks()
    {
        if (_tankPrefab == null || stopPoints.Count == 0)
        {
            Debug.LogError("Setup failed: Missing required references.");
            return;
        }

        if (_levelDataSO.TankDataList == null || _levelDataSO.TankDataList.Count == 0)
        {
            Debug.LogError("Level data contains no tank configurations.");
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

    public void MoveOtherTanks()
    {
        foreach (var tank in tankQueue)
        {
            Vector3 currentPosition = tank.transform.position;
            Vector3 targetPosition = new Vector3(currentPosition.x - TankSpacing, currentPosition.y, currentPosition.z);

            if (currentPosition.x > targetPosition.x)
            {
                tank.transform.DOMove(targetPosition, 3f).SetEase(Ease.Linear);
            }
        }

        if (currentTank != null)
        {
            Vector3 currentTankPosition = currentTank.transform.position;
            Vector3 targetCurrentTankPosition = new Vector3(currentTankPosition.x - TankSpacing, currentTankPosition.y, currentTankPosition.z);

            if (currentTankPosition.x > targetCurrentTankPosition.x)
            {
                currentTank.transform.DOMove(targetCurrentTankPosition, 3f).SetEase(Ease.Linear);
            }
        }
    }

    public Tank GetCurrentTank()
    {
        return currentTank;
    }
}
