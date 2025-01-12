using _Main._Enums;
using Array2DEditor;
using UnityEngine;

namespace LevelEditor
{
	[System.Serializable]
	public class Array2DGrid : Array2D<ColorType>
	{
		[SerializeField] private CellRowGrid[] cells = new CellRowGrid[Consts.DEFAULT_GRID_SIZE];

		protected override CellRow<ColorType> GetCellRow(int idx)
		{
			return cells[idx];
		}
	}

	[System.Serializable]
	public class CellRowGrid : CellRow<ColorType>
	{
	}
}