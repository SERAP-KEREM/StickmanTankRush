using _Main._Tank;
using LevelEditor;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [Title("Grid Configuration")]
    [SerializeField] private LevelDataSO _levelDataSO; // Tank verilerini tutan LevelDataSO
    [SerializeField] private Tank _tankPrefab; // Tank prefab referans?

    [SerializeField] private List<Transform> stopPoints; // StopPoint'lerin listesi

    // Tanklar aras?ndaki bo?luk miktar?
    private const float TankSpacing = 10f;

    // Ba?lang?ç konumu (tanklar?n s?ralanaca?? yer)
    [SerializeField] private Vector3 startPosition = new Vector3(10, -3, -40);

    // Ba?lang?ç rotas?
    [SerializeField] private float startRotationY = 90f; // Tanklar?n Y ekseninde ba?layaca?? rotas?

    private Queue<Tank> tankQueue = new Queue<Tank>();
    // Start is called before the first frame update
    void Start()
    {
        Setup();
        MoveNextTankToStopPoint();
    }

    public void Setup()
    {
        List<TankData> tankDataList = _levelDataSO.TankDataList; // Tank verilerini içeren liste

        for (int i = 0; i < tankDataList.Count; i++)
        {
            // Z koordinat?n? hesaplama (her tank aras?nda 2 birim bo?luk olacak)
            float z = startPosition.z + (i * TankSpacing); // Ba?lang?ç noktas?na ekleyerek s?ral?yoruz

            // Tanklar? s?ras?yla konumland?r?yoruz, X ve Y sabit kalacak
            Vector3 position = new Vector3(startPosition.x, startPosition.y, z);

            // Rotasyon ayar? (Tanklar Y ekseninde dönecek)
            Quaternion rotation = Quaternion.Euler(0, startRotationY, 0);

            // Tank prefab'?n? olu?turma
            Tank tank = Instantiate(_tankPrefab, position, rotation, transform);
            tank.UnitColorType = tankDataList[i].TankColorType; // Tank?n rengini ayarlama
            tankQueue.Enqueue(tank); // Tanklar? s?raya ekle
            tank.Initialize(stopPoints[0].position);  // Tank? ba?latma

            // Tank ismini ayarlama
            tank.name = $"{tankDataList[i].TankColorType} Tank [{i}]";
        }
    }
    void MoveNextTankToStopPoint()
    {
        if (tankQueue.Count == 0) return;

        Tank nextTank = tankQueue.Dequeue();
        nextTank.Initialize(stopPoints[0].position); // ?lk StopPoint hedef olarak ayarlan?r
    }
}
