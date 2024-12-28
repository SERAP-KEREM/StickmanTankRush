using _Main._Enums;
using _Main._Stickman;
using _Main._Stickman.StickmanGrid;
using _Main._Tank;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace _Main
{
    /// <summary>
    /// GameManager handles the main logic of the game including Stickman selection, tank management, and grid operations.
    /// It coordinates Stickman movement, adding Stickmen to tanks, and ensures that the game state is managed properly.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Field References

        [Header("References")]
        [SerializeField, Tooltip("Reference to the Tank Manager.")]
        private TankManager tankManager;

        [SerializeField, Tooltip("Reference to the Stickman Grid.")]
        private StickmanGrid stickmanGrid;

        [SerializeField, Tooltip("Reference to the Tile Grid.")]
        private TileGrid tileGrid;

        [SerializeField, Tooltip("Reference to the Waiting Row Manager.")]
        private WaitingRowManager waitingRowManager;

        #endregion

        #region Private Fields

        private Stickman selectedStickman;

        #endregion

        #region Unity Lifecycle Methods

        private void Start()
        {
            // Validate if all necessary references are assigned
            ValidateReferences();

            // Initialize grid systems
            stickmanGrid.Initialize();
            Debug.Log("StickmanGrid initialized.");

            tileGrid.Initialize();
            Debug.Log("TileGrid initialized.");
        }

        private void Update()
        {
            // Handle mouse clicks
            HandleMouseClick();
        }

        #endregion

        #region Reference Validation

        /// <summary>
        /// Validates that all required references are assigned.
        /// Logs an error if any required reference is missing.
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

        #endregion

        #region Stickman Handling

        /// <summary>
        /// Detects mouse clicks and selects a Stickman if clicked.
        /// Processes Stickman addition to tank or waiting row.
        /// </summary>
        private void HandleMouseClick()
        {
            if (Input.GetMouseButtonDown(0)) // Left-click detection
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Stickman clickedStickman = hit.collider.GetComponent<Stickman>();
                    if (clickedStickman != null && clickedStickman.IsSelectable)
                    {
                        selectedStickman = clickedStickman;
                        Debug.Log($"Selected Stickman: {clickedStickman.name}");

                        // Process Stickman addition to tank or waiting row
                        CheckAndAddStickmanToTank(selectedStickman);
                    }
                    else
                    {
                        Debug.Log("Selected object is not a selectable Stickman.");
                    }
                }
            }
        }

        #endregion

        #region Stickman Tank Addition

        /// <summary>
        /// Checks if the selected Stickman can be added to the tank and handles the addition logic.
        /// If the tank color mismatches and there's an empty neighboring space, sends the Stickman to a holder.
        /// </summary>
        /// <param name="stickman">The selected Stickman.</param>
        private void CheckAndAddStickmanToTank(Stickman stickman)
        {
            Tank currentTank = tankManager.GetCurrentTank();
            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            Debug.Log($"Current tank color: {currentTank.UnitColorType}, Stickman color: {stickman.UnitColorType}");

            // Check if Stickman is eligible to be added to the tank
            if (currentTank.UnitColorType != stickman.UnitColorType && tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                HandleColorMismatch(stickman);
                stickman.IsSelectable = false;
                return;
            }

            // If tank color matches or no empty neighbors, proceed with tank addition
            if (tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                MoveStickmanToTank(stickman, currentTank);
            }
            else
            {
                Debug.LogWarning("No empty neighboring grid spaces for the Stickman.");
            }
        }

        /// <summary>
        /// Handles the scenario when there is a color mismatch and moves the Stickman to an available holder.
        /// </summary>
        /// <param name="stickman">The Stickman to be moved.</param>
        private void HandleColorMismatch(Stickman stickman)
        {
            // Clear the tile where the Stickman was previously
            Tile currentTile = tileGrid.GetTileAt(stickman.GridX, stickman.GridY);
            if (currentTile != null)
            {
                currentTile.RemoveStickman();  // Remove Stickman from the old tile
                Debug.Log($"Tile at ({stickman.GridX}, {stickman.GridY}) is now empty.");
            }

            // Move Stickman to the new holder
            Holder nearestHolder = waitingRowManager.MoveToNearestAvailableHolder(stickman);
            if (nearestHolder != null)
            {
                nearestHolder.AssignStickman(stickman);  // Place the Stickman into the holder
                Debug.Log($"Stickman {stickman.name} moved to Holder {nearestHolder.name}.");
            }
            else
            {
                Debug.LogWarning("No available holder found for the Stickman.");
            }
        }

        /// <summary>
        /// Moves the Stickman to the tank and updates tank state accordingly.
        /// </summary>
        /// <param name="stickman">The Stickman to be moved.</param>
        /// <param name="currentTank">The tank where Stickman is added.</param>
        private void MoveStickmanToTank(Stickman stickman, Tank currentTank)
        {
            stickman.MoveToTank(currentTank.transform.position);
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

        #endregion

        #region Tank Management
        /// <summary>
        /// Moves the next tank to the stop point.
        /// </summary>
        private void MoveNextTankToStopPoint()
        {
            // Get the current tank
            Tank currentTank = tankManager.GetCurrentTank();
            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            Debug.Log("Moving to the next tank.");
            tankManager.MoveNextTankToStopPoint(); // Move the next tank to its destination
            tankManager.MoveOtherTanks(); // Move other tanks accordingly

            MoveAllHolderStickmenToCurrentTank();
        }

        /// <summary>
        /// Moves all matching Stickmen from holders to the current tank.
        /// </summary>
        private void MoveAllHolderStickmenToCurrentTank()
        {
            // Get the current tank
            Tank currentTank = tankManager.GetCurrentTank();
            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            // Get Stickmen from holders
            List<Stickman> stickmenInHolders = waitingRowManager.GetAllStickmenInHolders();
            Debug.Log($"Total Stickmen in holders: {stickmenInHolders.Count}");

            // Process each Stickman
            foreach (var stickman in stickmenInHolders)
            {
                if (stickman != null)
                {
                    Debug.Log($"Checking Stickman: {stickman.name}");
                    Debug.Log($"Stickman Color: {stickman.UnitColorType}, Tank Color: {currentTank.UnitColorType}");
                    // Move Stickman to the tank
                    if (stickman.UnitColorType == currentTank.UnitColorType)
                    {
                        MoveStickmanToTank(stickman, currentTank);
                    }
                    else
                    {
                        Debug.Log($"Skipping Stickman {stickman.name} due to color mismatch.");
                    }
                }
            }
        }

        #endregion
    }
}
