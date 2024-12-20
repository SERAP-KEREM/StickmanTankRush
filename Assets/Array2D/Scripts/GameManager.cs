using _Main._Enums;
using _Main._Stickman.StickmanGrid;
using _Main._Tank;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Tank Manager")]
    [SerializeField, Tooltip("Reference to the Tank Manager responsible for managing tanks.")]
    private TankManager tankManager; // Tank manager reference

    [Header("Stickman Click Event")]
    [SerializeField, Tooltip("Reference to the currently selected Stickman.")]
    private Stickman selectedStickman; // Referans seçilen Stickman'a

    [SerializeField, Tooltip("Reference to the StickmanGrid for checking neighboring grid spaces.")]
    private StickmanGrid stickmanGrid; // StickmanGrid referans? (kom?u bo?luklar? kontrol etmek için)

    private void Start()
    {
        // Ba?lang?çta yap?lacak i?lemler
        if (tankManager == null)
        {
            Debug.LogError("TankManager is not assigned!");
            return;
        }

        if (stickmanGrid == null)
        {
            Debug.LogError("StickmanGrid is not assigned!");
            return;
        }
    }

    private void Update()
    {
        // Stickman'a t?klan?p t?klanmad???n? kontrol ediyoruz
        if (selectedStickman != null)
        {
            // E?er seçilen stickman ve tank?n rengi uyuyorsa
            CheckAndAddStickmanToTank(selectedStickman);
        }
    }

    /// <summary>
    /// Stickman'?n tanka binebilmesi için renk uyumunu kontrol eder ve ekler.
    /// </summary>
    /// <param name="stickman">Seçilen Stickman</param>
    public void CheckAndAddStickmanToTank(Stickman stickman)
    {
        // TankManager'dan o anki aktif tank? al?yoruz
        Tank currentTank = tankManager.GetCurrentTank();

        // E?er aktif bir tank yoksa veya tank doluyorsa, i?lem yap?lmaz
        if (currentTank == null)
        {
            Debug.Log("No active tank at the moment!");
            return;
        }

        // Stickman'?n kom?u gridlerinde bo?luk var m? kontrol et
        if (!stickmanGrid.AreNeighborsEmpty(stickman.GridX, stickman.GridY))
        {
            Debug.Log("No empty space for the Stickman.");
            return;
        }

        // Tank?n rengi ile Stickman'?n rengini kontrol ediyoruz
        if (currentTank.UnitColorType == stickman.UnitColorType)
        {
            Debug.Log($"Stickman color ({stickman.UnitColorType}) matches the Tank color. Stickman is boarding the tank.");

            // Stickman'? tanka ekliyoruz
            currentTank.AddStickman(stickman.UnitColorType);

            // Tank dolarsa bir sonraki tanka geç
            if (currentTank.stickmanCount >= 3)
            {
                MoveNextTankToStopPoint();
            }
        }
        else
        {
            Debug.Log($"Color mismatch! Stickman color ({stickman.UnitColorType}) does not match Tank color ({currentTank.UnitColorType}).");
        }
    }

    /// <summary>
    /// Tank? bir sonraki durak noktas?na ta??r.
    /// </summary>
    public void MoveNextTankToStopPoint()
    {
        // TankManager'dan bir sonraki tank? durak noktas?na ta??yoruz
        tankManager.MoveNextTankToStopPoint();
    }
}
