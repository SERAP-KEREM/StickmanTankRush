using _Input;
using _Main._Stickman;
using _Main._Stickman.StickmanGrid;
using _Main._Tank;
using System.Collections.Generic;
using UnityEngine;

namespace _Main
{
    public class GameManager : MonoBehaviour
    {
        #region Field References
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField, Tooltip("Tank manager for managing tanks.")]
        public TankManager tankManager; // Public: TankManager reference

        [SerializeField, Tooltip("Stickman grid for managing stickmen.")]
        public StickmanGrid stickmanGrid; // Public: StickmanGrid reference

        [SerializeField, Tooltip("Tile grid for managing tiles.")]
        public TileGrid tileGrid; // Public: TileGrid reference

        [SerializeField, Tooltip("Holder manager for managing waiting row of stickmen.")]
        public HolderManager waitingRowManager; // Public: Waiting row manager reference

        #endregion

        #region Private Fields
        [Header("Private Fields")]
        private Stickman _selectedStickman; // Private: Selected Stickman
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject); // Singleton: Destroy other copies
            }
        }

        private void Start()
        {
            // Validate references
            ValidateReferences();

            // Initialize grid systems
            stickmanGrid.Initialize();
            Debug.Log("StickmanGrid initialized.");

            tileGrid.Initialize();
            Debug.Log("TileGrid initialized.");
        }

        private void Update()
        {
            // Handle input
            InputHandler.Instance.CheckInput();
        }

        #endregion

        #region Reference Validation
        private void ValidateReferences()
        {
            if (tankManager == null) { Debug.LogError("TankManager is not assigned!"); return; }
            if (stickmanGrid == null) { Debug.LogError("StickmanGrid is not assigned!"); return; }
            if (tileGrid == null) { Debug.LogError("TileGrid is not assigned!"); return; }

            Debug.Log("References validated.");
        }
        #endregion

        #region Stickman Handling
        public void HandleStickmanSelection(Stickman stickman)
        {
            _selectedStickman = stickman;
            Debug.Log($"Selected Stickman: {stickman.name}");

            // Check and add Stickman to tank or waiting row
            CheckAndAddStickmanToTank(stickman);
        }

        private void CheckAndAddStickmanToTank(Stickman stickman)
        {
            Tank currentTank = tankManager.CurrentTank;

            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            // Check color mismatch for Stickman
            if (currentTank.UnitColorType != stickman.UnitColorType && tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                HandleColorMismatch(stickman);
                return;
            }

            // Check if Stickman can be added to the tank
            if (tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                MoveStickmanToTank(stickman, currentTank);
            }
            else
            {
                Debug.LogWarning("No empty neighboring grid spaces for the Stickman.");
            }

            // Tank's holder checks
            if (currentTank.UnitColorType != stickman.UnitColorType)
            {
                HandleColorMismatch(stickman);
            }
        }

        private void HandleColorMismatch(Stickman stickman)
        {
            // Remove Stickman from the current tile
            Tile currentTile = tileGrid.GetTileAt(stickman.GridX, stickman.GridY);
            if (currentTile != null)
            {
                currentTile.RemoveStickman();
                Debug.Log($"Tile at ({stickman.GridX}, {stickman.GridY}) is now empty.");
            }

            // Move Stickman to holder
            Holder nearestHolder = waitingRowManager.MoveToNearestAvailableHolder(stickman);
            if (nearestHolder != null)
            {
                nearestHolder.AssignStickman(stickman);
                stickman.IsSelectable = false; // Cannot be selected once moved to holder
                Debug.Log($"Stickman {stickman.name} moved to Holder {nearestHolder.name}.");
            }
            else
            {
                Debug.LogWarning("No available holder found for the Stickman.");
            }
        }

        private void MoveStickmanToTank(Stickman stickman, Tank currentTank)
        {
            stickman.MoveToTank(currentTank.transform.position);

            currentTank.AddStickman(stickman.UnitColorType);

            if (currentTank.IsFull)
            {
                Debug.Log("Tank is full.");
                MoveNextTankToStopPoint();
            }
            else
            {
                Debug.Log("Tank is not full yet.");
            }
        }

        private void MoveNextTankToStopPoint()
        {
            // Get the current tank
            Tank currentTank = tankManager.CurrentTank;

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
            Tank currentTank = tankManager.CurrentTank;

            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            // Get all Stickmen in the holder
            List<Holder> allHolders = waitingRowManager.GetAllHolders();
            foreach (var holder in allHolders)
            {
                Stickman stickmanInHolder = holder.CurrentStickman;
                if (stickmanInHolder != null)
                {
                    // Move the stickman to the current tank if the tank is not full
                    if (!currentTank.IsFull)
                    {
                        // Remove Stickman from holder
                        holder.RemoveStickman();

                        // Move Stickman to the Tank
                        MoveStickmanToTank(stickmanInHolder, currentTank);

                        // Set Stickman as non-selectable once it is placed in the tank
                        stickmanInHolder.IsSelectable = false;
                        Debug.Log($"Stickman {stickmanInHolder.name} moved from holder to tank.");
                    }
                    else
                    {
                        Debug.Log("Tank is full, no more Stickmen can be added.");
                    }
                }
            }
        }
        #endregion
    }
}
