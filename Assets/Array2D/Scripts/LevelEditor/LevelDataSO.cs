using System;
using System.Collections.Generic;
using _Main._Enums;
using TriInspector;
using UnityEngine;

namespace LevelEditor
{
	[CreateAssetMenu(fileName = "Level_00", menuName = "StickmanTankRush/LevelData", order = 0)]
	public class LevelDataSO : ScriptableObject
	{
		[Title("Grid")]
		[SerializeField]
		private Array2DGrid _array2DGrid;
		public Array2DGrid Array2DGrid { get => _array2DGrid; set => _array2DGrid = value; }
      

        [Title("Tank")]
		[SerializeField] 
		private List<TankData> _tankDataList=new List<TankData>();
        public List<TankData> TankDataList { get => _tankDataList; }
    }

	[System.Serializable]
	public class TankData
	{
		public ColorType TankColorType;
	}
}