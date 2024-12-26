using _Main._Enums;
using _Main._Stickman;
using _Main._Stickman.StickmanGrid;
using _Main._Tank;
using DG.Tweening;
using UnityEngine;
using UnityEngine.WSA;

namespace _Main
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Reference to the Tank Manager.")]
        private TankManager tankManager;

        [SerializeField, Tooltip("Reference to the Stickman Grid.")]
        private StickmanGrid stickmanGrid;

        [SerializeField, Tooltip("Reference to the Tile Grid.")]
        private TileGrid tileGrid;

        private Stickman selectedStickman;
        [SerializeField] private WaitingRowManager waitingRowManager;


        private void Start()
        {
            ValidateReferences();

            stickmanGrid.Initialize();
            Debug.Log("StickmanGrid initialized.");

            tileGrid.Initialize();
            Debug.Log("TileGrid initialized.");
        }

        private void Update()
        {
            HandleMouseClick();
        }

        /// <summary>
        /// Validates required references and logs errors if any are missing.
        /// </summary>
        private void ValidateReferences()
        {
            if (tankManager == null)
            {
                Debug.LogError("TankManager is not assigned in the GameManager!");
                return;
            }
            Debug.Log("TankManager is assigned.");

            if (stickmanGrid == null)
            {
                Debug.LogError("StickmanGrid is not assigned in the GameManager!");
                return;
            }
            Debug.Log("StickmanGrid is assigned.");

            if (tileGrid == null)
            {
                Debug.LogError("TileGrid is not assigned in the GameManager!");
                return;
            }
            Debug.Log("TileGrid is assigned.");
        }

        /// <summary>
        /// Detects mouse clicks and selects a Stickman if clicked.
        /// </summary>
        private void HandleMouseClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse click detected.");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.Log("Raycast hit detected.");

                    Stickman clickedStickman = hit.collider.GetComponent<Stickman>();

                    if (clickedStickman != null && clickedStickman.IsSelectable)
                    {
                        Debug.Log($"Selectable Stickman clicked: {clickedStickman.name}");
                        selectedStickman = clickedStickman;
                        CheckAndAddStickmanToTank(selectedStickman);
                    }
                    else
                    {
                        Debug.Log("Clicked object is not a selectable Stickman.");
                    }
                }
                else
                {
                    Debug.Log("Raycast did not hit any object.");
                }
            }
        }

        /// <summary>
        /// Checks if the selected Stickman can be added to the tank and handles the addition.
        /// If the tank color mismatches and there's an empty neighboring space, sends the Stickman to a holder.
        /// </summary>
        /// <param name="stickman">The selected Stickman.</param>
        private void CheckAndAddStickmanToTank(Stickman stickman)
        {
            Debug.Log($"Checking if Stickman {stickman.name} can be added to the tank.");

            // Get the current tank (handle potential null case)
            Tank currentTank = tankManager.GetCurrentTank();
            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            Debug.Log($"Current tank color: {currentTank.UnitColorType}, Stickman color: {stickman.UnitColorType}");

            // Check tank color match and neighboring space
            bool areNeighborsEmpty = tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY);
            if (currentTank.UnitColorType != stickman.UnitColorType && areNeighborsEmpty)
            {
                Debug.Log($"Color mismatch! Sending Stickman {stickman.name} to a holder.");

                // Find the nearest available holder (ensure proper parameter passing)
                Holder nearestHolder = waitingRowManager.MoveToNearestAvailableHolder(stickman);

                // Move the stickman to the nearest holder (if found)
                if (nearestHolder != null)
                {
                   // Bu k?s?mda stickmanlar holdera gitti?inde sadece x ve z konumlar? de?i?sin y sabit kals?n

                    nearestHolder.AssignStickman(stickman);
                }
                else
                {
                    Debug.Log("No available holder found for the Stickman.");
                    // Handle case where no holder is available (optional: play animation, show message)
                }
                return;
            }

            // If color matches or no empty neighbors, proceed with tank addition logic
            if (!areNeighborsEmpty)
            {
                Debug.Log("No empty neighboring grid spaces for the Stickman.");
                return;
            }

            Debug.Log($"Stickman {stickman.name} is moving to the tank.");
            stickman.MoveToTank(currentTank.transform.position); // Assuming Stickman has a MoveToTank method
            currentTank.AddStickman(stickman.UnitColorType);

            if (currentTank.IsFull())
            {
                Debug.Log("Tank is full.");
                MoveNextTankToStopPoint();
            }
            else
            {
                Debug.Log("Tank is not full yet.");
            }
        }
        /// <summary>
        /// Moves the next tank to the stop point.
        /// </summary>
        private void MoveNextTankToStopPoint()
        {
            Debug.Log("Moving to the next tank.");
            tankManager.MoveNextTankToStopPoint();
            tankManager.MoveOtherTanks();
        }
    }
}
