using UnityEngine;

namespace Array2DEditor
{
    [System.Serializable]
    public class Array2DAudioClip : Array2D<AudioClip>
    {
        [SerializeField]
        CellRowAudioClip[] cells = new CellRowAudioClip[Consts.DEFAULT_GRID_SIZE];

        protected override CellRow<AudioClip> GetCellRow(int idx)
        {
            return cells[idx];
        }
    }
}