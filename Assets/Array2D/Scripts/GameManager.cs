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

            if (stickmanGrid == null)
            {
                Debug.LogError("StickmanGrid is not assigned in the GameManager!");
                return;
            }
            stickmanGrid.Initialize();
            tileGrid.Initialize();


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
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Stickman clickedStickman = hit.collider.GetComponent<Stickman>();

                    if (clickedStickman != null && clickedStickman.IsSelectable)
                    {
                        selectedStickman = clickedStickman;
                        CheckAndAddStickmanToTank(selectedStickman);
                    }
                }
            }
        }

        /// <summary>
        /// Stickman'?n tanka binebilmesi için gerekli kontrolleri yapar.
        /// </summary>
        /// <param name="stickman">Seçilen Stickman</param>
        private void CheckAndAddStickmanToTank(Stickman stickman)
        {
            // Aktif tank? al
            Tank currentTank = tankManager.GetCurrentTank();

            // Tank veya Stickman seçimi geçerli mi?
            if (currentTank == null)
            {
                Debug.Log("No active tank available.");
                return;
            }

            // Renk uyumunu kontrol et
            if (currentTank.UnitColorType != stickman.UnitColorType)
            {
                Debug.Log($"Color mismatch! Stickman color: {stickman.UnitColorType}, Tank color: {currentTank.UnitColorType}");
                return;
            }

            // Kom?u hücrelerin bo?lu?unu kontrol et
            if (!stickmanGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
            {
                Debug.Log("No empty neighboring grid spaces for the Stickman.");
                return;
            }

            // E?er tüm kontroller geçilirse, Stickman'? tanka ekle
            stickman.MoveToTank(currentTank.transform.position);
            currentTank.AddStickman(stickman.UnitColorType);

            // Tank dolmu?sa s?radaki tank? durak noktas?na götür
            if (currentTank.IsFull())
            {
                MoveNextTankToStopPoint();
            }
        }

        /// <summary>
        /// Bir sonraki tank? durak noktas?na ta??r.
        /// </summary>
        private void MoveNextTankToStopPoint()
        {
            tankManager.MoveNextTankToStopPoint();
            Debug.Log("Moving to the next tank.");
        }
    }
}
