using _Input;
using _Main._Stickman;
using _Main._Stickman.StickmanGrid;
using _Main._Tank;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;  // Make sure DOTween is properly included for animations

namespace _Main
{
    /// <summary>
    /// Main controller for managing the game logic, Stickman interactions, tanks, and movement.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Field References

        public static GameManager Instance { get; private set; }

        [Header("Game References")]
        [SerializeField, Tooltip("Manages all tank operations in the game.")]
        private TankManager _tankManager;

        [SerializeField, Tooltip("Handles the Stickman grid structure.")]
        private StickmanGrid _stickmanGrid;

        [SerializeField, Tooltip("Handles the Tile grid structure.")]
        private TileGrid _tileGrid;

        [SerializeField, Tooltip("Manages the holders for waiting Stickmen.")]
        private HolderManager _waitingRowManager;

        #endregion

        #region Private Fields

        [Header("Private State Variables")]
        [SerializeField, Tooltip("Currently selected Stickman.")]
        private Stickman _selectedStickman;

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
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Validate references at the start
            ValidateReferences();

            // Initialize grid systems (Stickman and Tile Grids)
            _stickmanGrid.Initialize();
            Debug.Log("StickmanGrid initialized.");

            _tileGrid.Initialize();
            Debug.Log("TileGrid initialized.");
        }

        #endregion

        #region Reference Validation

        /// <summary>
        /// Validates the assigned references for required components.
        /// </summary>
        private void ValidateReferences()
        {
            if (_tankManager == null) { Debug.LogError("TankManager is not assigned!"); return; }
            if (_stickmanGrid == null) { Debug.LogError("StickmanGrid is not assigned!"); return; }
            if (_tileGrid == null) { Debug.LogError("TileGrid is not assigned!"); return; }

            Debug.Log("References validated.");
        }

        #endregion

        #region Stickman Handling

        /// <summary>
        /// Handles the selection of a Stickman by the player.
        /// </summary>
        /// <param name="stickman">The selected Stickman instance.</param>
        public void HandleStickmanSelection(Stickman stickman)
        {
            _selectedStickman = stickman;
            Debug.Log($"Selected Stickman: {stickman.name}");

            // Check and add Stickman to the appropriate tank or waiting row
            CheckAndAddStickmanToTank(stickman);
        }

        /// <summary>
        /// Checks and adds the Stickman to the appropriate Tank or Holder.
        /// </summary>
        /// <param name="stickman">The Stickman to process.</param>
        private void CheckAndAddStickmanToTank(Stickman stickman)
        {
            Tank currentTank = _tankManager.CurrentTank;

            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            // Check for color mismatch or availability in neighboring tiles
            if (currentTank.UnitColorType != stickman.UnitColorType && _tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                HandleColorMismatch(stickman);
                return;
            }

            // Check if Stickman can be added to the tank
            if (_tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                MoveStickmanToTank(stickman, currentTank);
            }
            else
            {
                Debug.LogWarning("No empty neighboring grid spaces for the Stickman.");
            }
        }

        /// <summary>
        /// Handles the color mismatch situation and moves the Stickman to the nearest available holder.
        /// </summary>
        /// <param name="stickman">The Stickman to move.</param>
        private void HandleColorMismatch(Stickman stickman)
        {
            // Move Stickman to the nearest holder
            Holder nearestHolder = _waitingRowManager.MoveToNearestAvailableHolder(stickman);
            if (nearestHolder != null)
            {
                ProcessStickmanMovement(stickman, null, nearestHolder);
            }
            else
            {
                Debug.LogWarning("No available holder found for the Stickman.");
            }
        }

        /// <summary>
        /// Moves the Stickman to the appropriate Tank.
        /// </summary>
        /// <param name="stickman">The Stickman to move.</param>
        /// <param name="currentTank">The tank to move the Stickman to.</param>
        private void MoveStickmanToTank(Stickman stickman, Tank currentTank)
        {
            ProcessStickmanMovement(stickman, currentTank);

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

        /// <summary>
        /// Processes the movement of a Stickman (either to the Tank or Holder).
        /// </summary>
        /// <param name="stickman">The Stickman to move.</param>
        /// <param name="currentTank">The current Tank (optional, can be null for Holder).</param>
        /// <param name="nearestHolder">The nearest available holder (optional, can be null).</param>
        private void ProcessStickmanMovement(Stickman stickman, Tank currentTank, Holder nearestHolder = null)
        {
            // 1. Remove Stickman from grid.
            Tile currentTile = _tileGrid.GetTileAt(stickman.GridX, stickman.GridY);
            if (currentTile != null)
            {
                currentTile.RemoveStickman();
                Debug.Log($"Tile at ({stickman.GridX}, {stickman.GridY}) is now empty.");
            }

            // 2. Move Stickman to the new position.
            if (nearestHolder != null)
            {
                // Stickman is moved to the waiting row (holder).
                nearestHolder.AssignStickman(stickman);
                stickman.IsSelectable = false; // Stickman in the holder cannot be selected.
                Debug.Log($"Stickman {stickman.name} moved to Holder {nearestHolder.name}.");
            }
            else if (currentTank != null)
            {
                // Stickman is moved to the tank.
                stickman.MoveToTank(currentTank.transform.position);
                currentTank.AddStickman(stickman.UnitColorType);
                Debug.Log($"Stickman {stickman.name} moved to Tank.");
            }
        }

        private void MoveNextTankToStopPoint()
        {
            Tank currentTank = _tankManager.CurrentTank;

            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            Debug.Log("Moving to the next tank.");
            _tankManager.MoveNextTankToStopPoint(); // Move the next tank to its stop point
            _tankManager.MoveOtherTanks(); // Reorganize other tanks

            MoveAllHolderStickmenToCurrentTank();
        }

        private void MoveAllHolderStickmenToCurrentTank()
        {
            Tank currentTank = _tankManager.CurrentTank;

            if (currentTank == null)
            {
                Debug.LogWarning("No active tank available.");
                return;
            }

            List<Holder> allHolders = _waitingRowManager.GetAllHolders();
            foreach (var holder in allHolders)
            {
                Stickman stickmanInHolder = holder.CurrentStickman;
                if (stickmanInHolder != null && !currentTank.IsFull)
                {
                    holder.RemoveStickman();
                    MoveStickmanToTank(stickmanInHolder, currentTank);
                    stickmanInHolder.IsSelectable = false;
                    Debug.Log($"Stickman {stickmanInHolder.name} moved from holder to tank.");
                }
                else if (currentTank.IsFull)
                {
                    Debug.Log("Tank is full, no more Stickmen can be added.");
                }
            }
        }

        #endregion
    }
}
