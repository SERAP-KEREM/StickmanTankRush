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

    // Tanklar aras? bo?luk miktar?
    private const float TankSpacing = 2f;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    public void Setup()
    {
        List<TankData> tankDataList = _levelDataSO.TankDataList; // Tek boyutlu tank listesi

        for (int i = 0; i < tankDataList.Count; i++)
        {
            // X koordinat?n? hesaplama (her tank aras?nda 2 birim bo?luk olacak)
            float x = i * TankSpacing;

            // Pozisyon belirleme
            Vector3 position = new Vector3(x, 0, 0); // Sadece X ekseninde ilerleme

            // Tank prefab?n? olu?turma
            Tank tank = Instantiate(_tankPrefab, position, Quaternion.identity, transform);
            tank.UnitColorType = tankDataList[i].TankColorType; // Renk tipini ayarl?yoruz
            tank.Initialize(); // Tank? ba?lat?yoruz

            // Tank ismini ayarl?yoruz
            tank.name = $"Tank [{i}]";

            // Debug log
            Debug.Log($"Tank Created at X: {x} with Color: {tankDataList[i].TankColorType}");
        }
    }
}
