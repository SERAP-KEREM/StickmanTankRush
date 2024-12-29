using UnityEngine;
using LevelEditor;
using _Main._Stickman.StickmanGrid;

public class Level : MonoBehaviour
{
    [SerializeField]
    private TankManager tankManager;

    [SerializeField]
    private StickmanGrid stickmanGrid;

    [SerializeField]
    private LevelDataSO _levelDataSO;

    // Eri?im metotlar?
    public StickmanGrid StickmanGrid => stickmanGrid;
    public LevelDataSO LevelDataSO => _levelDataSO;

    private void Start()
    {
        // E?er StickmanGrid ve _levelDataSO bo?sa hata veriyoruz
        if (stickmanGrid == null || _levelDataSO == null)
        {
            Debug.LogError("Level is missing StickmanGrid or LevelDataSO references.");
        }

        // TankManager'a LevelDataSO ve StickmanGrid referanslar?n? gönderiyoruz
        tankManager.SetLevelDataSO(_levelDataSO);
       // tankManager.SetStickmanGrid(stickmanGrid);
    }
}
