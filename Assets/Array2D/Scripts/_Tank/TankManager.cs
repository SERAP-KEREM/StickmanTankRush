using _Main._Enums;
using _Main._Tank;
using LevelEditor;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [Title("Grid Configuration")]
    [SerializeField, Tooltip("SO that contains tank data like color types and other configurations.")]
    private LevelDataSO _levelDataSO; // The LevelDataSO that holds tank data

    [SerializeField, Tooltip("Prefab reference for the tank.")]
    private Tank _tankPrefab; // Tank prefab reference

    [SerializeField, Tooltip("List of stop points for the tanks to stop at.")]
    private List<Transform> stopPoints; // List of stop points

    // Tank queues and currently controlled tank
    private Queue<Tank> tankQueue = new Queue<Tank>();
    private Tank currentTank; // Currently controlled tank

    // Public variable to store the first tank in the queue
    public Tank firstTank;

    // Constants and initial positions
    private const float TankSpacing = 10f;
    [SerializeField, Tooltip("Initial start position for tanks.")]
    private Vector3 startPosition = new Vector3(10, -3, -40);

    [SerializeField, Tooltip("Initial rotation on the Y axis for the tanks.")]
    private float startRotationY = 90f;

    // New addition: Max stickman count per tank
    private const int MaxStickmanCount = 3;

    /// <summary>
    /// The Start method is called at the beginning of the game to initialize tanks.
    /// </summary>
    void Start()
    {
        Setup();
        MoveNextTankToStopPoint();  // Move the first tank to the stop point
    }

    /// <summary>
    /// Sets up the tanks based on data from LevelDataSO and prepares them for the game.
    /// </summary>
    public void Setup()
    {
        // Creates tanks from tank data
        List<TankData> tankDataList = _levelDataSO.TankDataList;

        for (int i = 0; i < tankDataList.Count; i++)
        {
            // Calculate the Z coordinate inversely
            float z = startPosition.z - (i * TankSpacing);

            // Tank position and rotation
            Vector3 position = new Vector3(startPosition.x, startPosition.y, z);
            Quaternion rotation = Quaternion.Euler(0, startRotationY, 0);

            // Instantiate the tank
            Tank tank = Instantiate(_tankPrefab, position, rotation, transform);
            tank.UnitColorType = tankDataList[i].TankColorType;
            tankQueue.Enqueue(tank);
            tank.Initialize(stopPoints[0].position);

            // Update tank's name
            tank.name = $"{tankDataList[i].TankColorType} Tank [{i}]";

            // If this is the first tank, assign it to the firstTank variable
            if (i == 0)
            {
                firstTank = tank;
            }
        }
    }

    /// <summary>
    /// Called every frame to handle user input and perform actions like printing the current tank info.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // Press "P" to print the current tank's info
        {
            PrintCurrentTankInfo();
        }
    }

    /// <summary>
    /// Moves the next tank from the queue to the stop point.
    /// </summary>
    public void MoveNextTankToStopPoint()
    {
        if (tankQueue.Count == 0) return;

        // If the current tank is full, start moving and move to the next tank.
        if (currentTank != null && currentTank.stickmanCount >= MaxStickmanCount)
        {
            currentTank.StartMoving();  // Start moving the current tank
        }

        // Get the next tank in the queue
        currentTank = tankQueue.Dequeue();
        currentTank.Initialize(stopPoints[0].position);  // Place the new tank at the stop point
    }

    /// <summary>
    /// Prints information about the current tank, such as its name and color.
    /// </summary>
    public void PrintCurrentTankInfo()
    {
        if (tankQueue.Count == 0 && currentTank == null)
        {
            Debug.Log("Tank list is empty!");
            return;
        }

        if (currentTank != null)
        {
            Debug.Log($"Current Tank: {currentTank.name}, Color: {currentTank.UnitColorType}, Stickman Count: {currentTank.stickmanCount}");
        }
        else
        {
            Debug.Log("No active tank at the moment!");
        }
    }

    /// <summary>
    /// Returns the currently active tank that is under control.
    /// </summary>
    /// <returns>The current tank being controlled.</returns>
    public Tank GetCurrentTank()
    {
        return currentTank;
    }

    /// <summary>
    /// Checks and adds a stickman to the current tank if the colors match and the tank isn't full.
    /// </summary>
    public void CheckAndAddStickmanToTank(ColorType stickmanColor)
    {
        if (currentTank == null)
        {
            Debug.Log("No active tank at the moment!");
            return;
        }

        // If the current tank is full, move to the next one.
        if (currentTank.stickmanCount >= MaxStickmanCount)
        {
            Debug.Log($"{currentTank.UnitColorType} tank is full. Moving to the next tank.");
            MoveNextTankToStopPoint();
            return;
        }

        // Check if the tank's color matches the stickman color.
        if (currentTank.UnitColorType == stickmanColor)
        {
            Debug.Log($"Stickman color matches the Tank color. Stickman is boarding the tank.");
            currentTank.AddStickman(stickmanColor);  // Add the stickman to the tank
        }
        else
        {
            Debug.Log($"Color mismatch! Stickman color does not match Tank color.");
        }
    }
}
