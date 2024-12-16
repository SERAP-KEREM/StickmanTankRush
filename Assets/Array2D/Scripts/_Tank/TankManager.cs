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

    // Tanklar aras?ndaki bo?luk miktar?
    private const float TankSpacing = 10f;

    // Ba?lang?ç konumu (tanklar?n s?ralanaca?? yer)
    [SerializeField] private Vector3 startPosition = new Vector3(0, 0, 0);

    // Ba?lang?ç rotas?
    [SerializeField] private float startRotationY = 0f; // Tanklar?n Y ekseninde ba?layaca?? rotas?

    // Start is called before the first frame update
    void Start()
    {
        Setup();
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
            tank.Initialize(); // Tank? ba?latma

            // Tank ismini ayarlama
            tank.name = $"{tankDataList[i].TankColorType} Tank [{i}]";
        }
    }
}
