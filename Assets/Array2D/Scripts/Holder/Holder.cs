using _Main._Stickman.StickmanGrid;
using UnityEngine;

public class Holder : MonoBehaviour
{
    public bool IsOccupied => AssignedStickman != null; // E?er bir Stickman atanm??sa doludur.
    public Stickman AssignedStickman { get; private set; } = null;

    /// <summary>
    /// Holder'a bir Stickman yerle?tirir.
    /// </summary>
    /// <param name="stickman">Yerle?tirilecek Stickman.</param>
    public void AssignStickman(Stickman stickman)
    {
        AssignedStickman = stickman;
        stickman.transform.position = transform.position; // Stickman'? Holder pozisyonuna ta??
    }
    public void PlaceStickman(Stickman stickman)
    {
        AssignedStickman = stickman;
    }
    /// <summary>
    /// Holder'? bo?alt?r ve içindeki Stickman'? serbest b?rak?r.
    /// </summary>
    public void Vacate()
    {
        if (AssignedStickman != null)
        {
            AssignedStickman = null;
        }
    }
   


}
