using _Main._Enums;
using _Main._Tank;
using LevelEditor;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [Title("Grid Configuration")]
    [SerializeField, Tooltip("Tank verilerini içeren ve renk türleri gibi yap?land?rmalar? bar?nd?ran SO.")]
    private LevelDataSO _levelDataSO; // Tank verilerini tutan LevelDataSO

    [SerializeField, Tooltip("Tank prefab'? referans?.")]
    private Tank _tankPrefab; // Tank prefab'? referans?

    [SerializeField, Tooltip("Tanklar?n duraca?? durak noktalar? listesi.")]
    private List<Transform> stopPoints; // Durak noktalar?n?n listesi

    // Tank kuyruklar? ve ?u an kontrol edilen tank
    private Queue<Tank> tankQueue = new Queue<Tank>();
    [SerializeField] private Tank currentTank; // ?u anki kontrol edilen tank

    // Sabitler ve ilk pozisyonlar
    private const float TankSpacing = 10f;
    [SerializeField, Tooltip("Tanklar için ba?lang?ç pozisyonu.")]
    private Vector3 startPosition = new Vector3(0, 0, 0);

    // Yeni eklenen: Tank ba??na maksimum stickman say?s?
    private const int MaxStickmanCount = 3;

    /// <summary>
    /// Start metodu, oyun ba??nda tanklar? ba?latmak için ça?r?l?r.
    /// </summary>
    void Start()
    {
        Setup();
        MoveNextTankToStopPoint();  // ?lk tank? durak noktas?na hareket ettir
    }

    /// <summary>
    /// LevelDataSO'dan al?nan verilere göre tanklar? kurar ve oyuna haz?rlar.
    /// </summary>
    public void Setup()
    {
        // Tank verilerini olu?tur
        List<TankData> tankDataList = _levelDataSO.TankDataList;

        for (int i = 0; i < tankDataList.Count; i++)
        {
            // Z koordinat?n? ters yönde hesapla
            float x = startPosition.x + (i * TankSpacing);

            // Tank pozisyonu ve rotas?
            Vector3 position = new Vector3(x, startPosition.y, startPosition.z);
            Quaternion rotation = Quaternion.Euler(0, 0, 0);

            // Tank? instantiate et
            Tank tank = Instantiate(_tankPrefab, position, rotation, transform);
            tank.UnitColorType = tankDataList[i].TankColorType;
            tankQueue.Enqueue(tank);
            tank.Initialize(stopPoints[0].position);

            // Tank'?n ad?n? güncelle
            tank.name = $"{tankDataList[i].TankColorType} Tank [{i}]";
        }
    }

    /// <summary>
    /// Her frame'de kullan?c? giri?lerini i?leyip, mevcut tank bilgilerini yazd?rma gibi i?lemleri yapar.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // "P" tu?una bas?ld???nda mevcut tank bilgisini yazd?r
        {
            PrintCurrentTankInfo();
        }
    }

    /// <summary>
    /// Kuyruktaki bir sonraki tank? durak noktas?na ta??r.
    /// </summary>
    public void MoveNextTankToStopPoint()
    {
        if (tankQueue.Count == 0) return;

        // E?er mevcut tank doluysa, hareket etmeye ba?la
        if (currentTank != null && currentTank.stickmanCount >= MaxStickmanCount)
        {
            currentTank.StartMoving();  // Mevcut tank? hareket ettirmeye ba?la
        }

        // Kuyruktaki bir sonraki tank? al
        if (currentTank != null && currentTank.stickmanCount >= MaxStickmanCount)
        {
            currentTank.currentState = TankState.Moving;
            // Tank doldu ve hareket etmeye ba?lad?, bir sonraki tank? al
            currentTank = tankQueue.Dequeue();
        }

        // E?er kuyrukta tank varsa, ilk tank? seçip durak noktas?na yerle?tir
        if (tankQueue.Count > 0)
        {
            currentTank = tankQueue.Peek();  // Kuyru?un ba??ndaki tank? al
            currentTank.Initialize(stopPoints[0].position);  // Yeni tank? durak noktas?na yerle?tir
        }
    }

    /// <summary>
    /// Mevcut tank hakk?nda bilgi yazd?r?r, örne?in ismi ve rengi.
    /// </summary>
    public void PrintCurrentTankInfo()
    {
        if (tankQueue.Count == 0 && currentTank == null)
        {
            Debug.Log("Tank listesi bo?!");
            return;
        }

        if (currentTank != null)
        {
            Debug.Log($"Mevcut Tank: {currentTank.name}, Renk: {currentTank.UnitColorType}, Stickman Say?s?: {currentTank.stickmanCount}");
        }
        else
        {
            Debug.Log("?u an aktif bir tank yok!");
        }
    }

    /// <summary>
    /// Kontrol alt?nda olan aktif tank? döndürür.
    /// </summary>
    /// <returns>Kontrol edilen aktif tank.</returns>
    public Tank GetCurrentTank()
    {
        return currentTank;
    }
}
