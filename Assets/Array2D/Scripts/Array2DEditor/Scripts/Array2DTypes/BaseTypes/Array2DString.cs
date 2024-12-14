using UnityEngine;

namespace Array2DEditor
{
    [System.Serializable]
    public class Array2DString : Array2D<string>
    {
        [SerializeField]
        CellRowString[] cells = new CellRowString[Consts.DEFAULT_GRID_SIZE];

        protected override CellRow<string> GetCellRow(int idx)
        {
            return cells[idx];
        }
    }
}
