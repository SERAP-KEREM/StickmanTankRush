using _Main._Enums;
using _Main._Stickman;
using _Main._Stickman.StickmanGrid;
using _Main._Tank;
using UnityEngine;

namespace _Main
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Reference to the Tank Manager.")]
        private TankManager tankManager;

        [SerializeField, Tooltip("Reference to the Stickman Grid.")]
        private StickmanGrid stickmanGrid;

        private Stickman selectedStickman;

        [SerializeField]
        private TileGrid tileGrid;

        private void Start()
        {
            // Referanslar?n atan?p atanmad???n? kontrol et
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

            stickmanGrid.Initialize();
            Debug.Log("StickmanGrid initialized.");

            tileGrid.Initialize();
            Debug.Log("TileGrid initialized.");

            // ?lk tank?n durumu kontrol edilir
            CheckAndInitializeCurrentTank();
        }

        private void Update()
        {
            // Fare t?klamalar?n? kontrol et ve Stickman'? seç
            HandleMouseClick();
        }

        /// <summary>
        /// Fare t?klamalar?n? alg?lar ve Stickman seçim i?lemini yapar.
        /// </summary>
        private void HandleMouseClick()
        {
            if (Input.GetMouseButtonDown(0)) // Sol t?klama
            {
                Debug.Log("Mouse click detected.");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.Log("Raycast hit something.");

                    Stickman clickedStickman = hit.collider.GetComponent<Stickman>();

                    if (clickedStickman != null && clickedStickman.IsSelectable)
                    {
                        Debug.Log($"Clicked on a selectable Stickman: {clickedStickman.name}");
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
                    Debug.Log("Raycast did not hit anything.");
                }
            }
        }

        /// <summary>
        /// Stickman'?n tanka binebilmesi için gerekli kontrolleri yapar.
        /// </summary>
        /// <param name="stickman">Seçilen Stickman</param>
        private void CheckAndAddStickmanToTank(Stickman stickman)
        {
            Debug.Log($"Checking if Stickman {stickman.name} can be added to the tank.");

            Tank currentTank = tankManager.GetCurrentTank();

            if (currentTank == null)
            {
                Debug.Log("No active tank available.");
                return;
            }

            Debug.Log($"Current tank color: {currentTank.UnitColorType}, Stickman color: {stickman.UnitColorType}");

            // Stickman ve tank renklerini kontrol et
            if (currentTank.UnitColorType != stickman.UnitColorType)
            {
                Debug.Log($"Color mismatch! Stickman color: {stickman.UnitColorType}, Tank color: {currentTank.UnitColorType}");
                return;
            }

            // Stickman yerle?ebilecek bo? bir alana sahip mi kontrol et
            if (!tileGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                Debug.Log("No empty neighboring grid spaces for the Stickman.");
                return;
            }

            Debug.Log($"Stickman {stickman.name} is moving to the tank.");
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

        /// <summary>
        /// Bir sonraki tank? durak noktas?na ta??r.
        /// </summary>
        private void MoveNextTankToStopPoint()
        {
            Debug.Log("Moving to the next tank.");
            tankManager.MoveNextTankToStopPoint();
        }

        /// <summary>
        /// Tank?n do?ru ?ekilde ba?lat?ld???ndan emin ol.
        /// </summary>
        private void CheckAndInitializeCurrentTank()
        {
            // ?lk tank? kontrol et ve durumu ayarla
            Tank currentTank = tankManager.GetCurrentTank();

            if (currentTank == null)
            {
                Debug.LogError("No tanks available.");
                return;
            }

            // E?er tank "Filling" durumundaysa, di?er tanklar beklemelidir
            if (currentTank.GetCurrentState() == TankState.Waiting)
            {
                Debug.Log("Tank is in Waiting state.");
            }
            else if (currentTank.GetCurrentState() == TankState.Filling)
            {
                Debug.Log("Tank is in Filling state.");
            }
            else if (currentTank.GetCurrentState() == TankState.Moving)
            {
                Debug.Log("Tank is in Moving state.");
            }
        }
    }
}
